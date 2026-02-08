using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Services;

public sealed class WizardDistributionService(
    IReleaseCatalogRepository releaseCatalogRepository,
    IArtifactStorage artifactStorage,
    IOptions<SupplyApiOptions> options
) : IWizardDistributionService
{
    private readonly SupplyApiOptions _supplyApiOptions = options.Value;

    public async Task<WizardBinaryLatestDocument?> GetLatestWizardBinaryAsync(
        string channel,
        string operatingSystem,
        string architecture,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        var catalog = await releaseCatalogRepository.GetCatalogAsync(cancellationToken);
        var policy = CatalogResolution.ResolveCustomerPolicy(catalog, _supplyApiOptions, customerContext);
        CatalogResolution.ValidateAccess(_supplyApiOptions, customerContext, channel, policy);

        var releaseId = CatalogResolution.ResolveWizardBinaryReleaseId(catalog, channel, policy);
        if (!catalog.WizardBinaryReleases.TryGetValue(releaseId, out WizardBinaryReleaseDocument? release))
        {
            return null;
        }

        var artifact =
            FindWizardArtifact(catalog, release, operatingSystem, architecture)
            ?? throw new ApiRequestException(
                $"No wizard binary is available for {operatingSystem}/{architecture} in release '{release.Id}'.",
                StatusCodes.Status404NotFound
            );

        var resolvedBaseUri = ResolveBaseUri(baseUri);
        return new WizardBinaryLatestDocument
        {
            Channel = channel,
            Version = release.Version,
            Os = artifact.Os,
            Architecture = artifact.Architecture,
            SizeBytes = artifact.SizeBytes,
            Sha256 = artifact.Sha256,
            DownloadUri = new Uri(
                resolvedBaseUri,
                $"/api/wizard/binaries/{Uri.EscapeDataString(release.Version)}/{Uri.EscapeDataString(artifact.Os)}/{Uri.EscapeDataString(artifact.Architecture)}"
            ),
            ETag = CreateStrongETag(artifact.Sha256),
            PublishedAtUtc = artifact.PublishedAtUtc,
        };
    }

    public async Task<ArtifactDownloadResult?> OpenWizardBinaryAsync(
        string version,
        string operatingSystem,
        string architecture,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        _ = customerContext;
        var catalog = await releaseCatalogRepository.GetCatalogAsync(cancellationToken);
        var matchingReleaseId = catalog
            .WizardBinaryReleases.Values.FirstOrDefault(release =>
                string.Equals(release.Version, version, StringComparison.OrdinalIgnoreCase)
            )
            ?.Id;

        if (
            matchingReleaseId is null
            || !catalog.WizardBinaryReleases.TryGetValue(matchingReleaseId, out WizardBinaryReleaseDocument? release)
        )
        {
            return null;
        }

        var artifact =
            FindWizardArtifact(catalog, release, operatingSystem, architecture)
            ?? throw new ApiRequestException(
                $"No wizard binary is available for {operatingSystem}/{architecture} at version '{version}'.",
                StatusCodes.Status404NotFound
            );

        return await OpenArtifactInternalAsync(artifact, cancellationToken);
    }

    public async Task<ArtifactDownloadResult?> OpenArtifactAsync(
        string artifactId,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        _ = customerContext;
        var catalog = await releaseCatalogRepository.GetCatalogAsync(cancellationToken);
        if (!catalog.Artifacts.TryGetValue(artifactId, out ArtifactDocument? artifact))
        {
            return null;
        }

        return await OpenArtifactInternalAsync(artifact, cancellationToken);
    }

    private async Task<ArtifactDownloadResult> OpenArtifactInternalAsync(
        ArtifactDocument artifact,
        CancellationToken cancellationToken
    )
    {
        var exists = await artifactStorage.ExistsAsync(artifact.RelativePath, cancellationToken);
        if (!exists)
        {
            throw new ApiRequestException(
                $"Artifact content for '{artifact.Id}' is unavailable in storage.",
                StatusCodes.Status404NotFound
            );
        }

        var metadata =
            await artifactStorage.GetMetadataAsync(artifact.RelativePath, cancellationToken)
            ?? throw new ApiRequestException(
                $"Artifact metadata for '{artifact.Id}' is unavailable in storage.",
                StatusCodes.Status404NotFound
            );
        var stream = await artifactStorage.OpenReadAsync(artifact.RelativePath, cancellationToken);
        return new ArtifactDownloadResult
        {
            ContentStream = stream,
            ContentType = artifact.ContentType,
            FileName = artifact.FileName,
            SizeBytes = metadata.SizeBytes,
            ETag = CreateStrongETag(artifact.Sha256),
            LastModifiedUtc = metadata.LastWriteAtUtc,
        };
    }

    private static ArtifactDocument? FindWizardArtifact(
        CatalogDocument catalog,
        WizardBinaryReleaseDocument release,
        string operatingSystem,
        string architecture
    )
    {
        foreach (var artifactId in release.ArtifactIds)
        {
            if (!catalog.Artifacts.TryGetValue(artifactId, out var artifact))
            {
                continue;
            }

            if (!string.Equals(artifact.Os, operatingSystem, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(artifact.Architecture, architecture, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return artifact;
        }

        return null;
    }

    private static Uri ResolveBaseUri(string? baseUri)
    {
        if (!string.IsNullOrWhiteSpace(baseUri) && Uri.TryCreate(baseUri, UriKind.Absolute, out var parsed))
        {
            return parsed;
        }

        return new UriBuilder(Uri.UriSchemeHttps, "localhost", 5001).Uri;
    }

    private static string CreateStrongETag(string sha256)
    {
        return $"\"sha256:{sha256}\"";
    }
}
