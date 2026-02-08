using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Services;

/// <summary>
/// Builds manifest responses from catalog releases and customer policy.
/// </summary>
public sealed class WizardManifestService(
    IReleaseCatalogRepository releaseCatalogRepository,
    IOptions<SupplyApiOptions> options
) : IWizardManifestService
{
    private readonly SupplyApiOptions _supplyApiOptions = options.Value;

    /// <summary>
    /// Gets a manifest for the requested channel and customer context.
    /// </summary>
    /// <param name="request">Manifest request.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Manifest envelope when found.</returns>
    public async Task<ManifestEnvelope?> GetManifestAsync(
        ManifestRequest request,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        var catalog = await releaseCatalogRepository.GetCatalogAsync(cancellationToken);
        var policy = CatalogResolution.ResolveCustomerPolicy(catalog, _supplyApiOptions, customerContext);
        CatalogResolution.ValidateAccess(_supplyApiOptions, customerContext, request.Channel, policy);

        var releaseId = CatalogResolution.ResolveManifestReleaseId(catalog, request.Channel, policy);
        if (!catalog.ManifestReleases.TryGetValue(releaseId, out ManifestReleaseDocument? release))
        {
            throw new ApiRequestException(
                $"Manifest release '{releaseId}' was not found.",
                StatusCodes.Status404NotFound
            );
        }

        var baseUri = ResolveBaseUri(request.BaseUri);
        var components = BuildComponents(release.Components, catalog.Artifacts, baseUri);
        var dependencies = release
            .Dependencies.Select(static dependency => new DependencyManifestDocument
            {
                Id = dependency.Id,
                DisplayName = dependency.DisplayName,
                DefaultPolicy = dependency.DefaultPolicy,
                ManagedComponentId = dependency.ManagedComponentId,
                ProbeScheme = dependency.ProbeScheme,
                ProbePort = dependency.ProbePort,
            })
            .ToList();

        var manifest = new WizardManifestDocument
        {
            ManifestId = $"{release.Id}:{customerContext.CustomerId}:{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            ManifestVersion = release.ReleaseVersion,
            PublishedAtUtc = DateTimeOffset.UtcNow,
            Channel = request.Channel,
            CustomerId = customerContext.CustomerId,
            ReleaseVersion = release.ReleaseVersion,
            MinWizardVersion = release.MinWizardVersion,
            Components = components,
            Dependencies = dependencies,
            Features = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["PhasedRolloutEnabled"] = true,
            },
        };

        return new ManifestEnvelope { Document = manifest, ETag = ManifestHashHelper.ComputeStrongETag(manifest) };
    }

    private static Uri ResolveBaseUri(string? baseUri)
    {
        if (!string.IsNullOrWhiteSpace(baseUri) && Uri.TryCreate(baseUri, UriKind.Absolute, out var parsedBaseUri))
        {
            return parsedBaseUri;
        }

        return new UriBuilder(Uri.UriSchemeHttps, "localhost", 5001).Uri;
    }

    private static List<ComponentManifestDocument> BuildComponents(
        IReadOnlyList<ManifestComponentDocument> components,
        Dictionary<string, ArtifactDocument> artifacts,
        Uri baseUri
    )
    {
        var result = new List<ComponentManifestDocument>();

        foreach (var component in components)
        {
            var artifactDocuments = new List<ArtifactManifestDocument>();
            foreach (var artifactId in component.ArtifactIds)
            {
                if (!artifacts.TryGetValue(artifactId, out var artifact))
                {
                    throw new ApiRequestException(
                        $"Artifact '{artifactId}' referenced by component '{component.Id}' does not exist.",
                        StatusCodes.Status500InternalServerError
                    );
                }

                var downloadUri = new Uri(baseUri, $"/api/wizard/artifacts/{Uri.EscapeDataString(artifact.Id)}");
                artifactDocuments.Add(
                    new ArtifactManifestDocument
                    {
                        ArtifactId = artifact.Id,
                        Os = artifact.Os,
                        Architecture = artifact.Architecture,
                        PackageType = artifact.PackageType,
                        DownloadUri = downloadUri,
                        FileName = artifact.FileName,
                        SizeBytes = artifact.SizeBytes,
                        Sha256 = artifact.Sha256,
                        ETag = CreateStrongETag(artifact.Sha256),
                    }
                );
            }

            result.Add(
                new ComponentManifestDocument
                {
                    Id = component.Id,
                    DisplayName = component.DisplayName,
                    Version = component.Version,
                    EnabledByDefault = component.EnabledByDefault,
                    DependsOnComponentIds = component.DependsOnComponentIds,
                    DependencyIds = component.DependencyIds,
                    Artifacts = artifactDocuments,
                    Service = new ServiceManifestDocument
                    {
                        ServiceName = component.Service.ServiceName,
                        DisplayName = component.Service.DisplayName,
                        ExecutablePath = component.Service.ExecutablePath,
                        Arguments = component.Service.Arguments,
                        WorkingDirectoryPath = component.Service.WorkingDirectoryPath,
                        EnvironmentVariables = component.Service.EnvironmentVariables,
                        DefaultInstallPaths = component.Service.DefaultInstallPaths,
                    },
                }
            );
        }

        return result;
    }

    private static string CreateStrongETag(string sha256)
    {
        return $"\"sha256:{sha256}\"";
    }
}
