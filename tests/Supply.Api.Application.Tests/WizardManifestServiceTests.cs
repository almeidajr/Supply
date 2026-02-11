using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Tests;

public sealed class WizardManifestServiceTests
{
    [Fact]
    public async Task GetManifestAsync_WhenCatalogIsValid_ShouldBuildManifestEnvelope()
    {
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateValidCatalog() };
        var service = CreateService(repository);
        var request = new ManifestRequest { Channel = "stable", BaseUri = "https://api.example.com/" };

        var envelope = await service.GetManifestAsync(request, CreateCustomerContext(), CancellationToken.None);

        Assert.NotNull(envelope);
        Assert.StartsWith("manifest-r1:contoso:", envelope.Document.ManifestId);
        Assert.Equal("stable", envelope.Document.Channel);
        Assert.Equal("contoso", envelope.Document.CustomerId);
        Assert.Equal("2026.02.1", envelope.Document.ReleaseVersion);
        Assert.True(envelope.Document.Features["PhasedRolloutEnabled"]);

        var component = Assert.Single(envelope.Document.Components);
        var artifact = Assert.Single(component.Artifacts);
        Assert.Equal("artifact-win-x64", artifact.ArtifactId);
        Assert.Equal(new Uri("https://api.example.com/api/wizard/artifacts/artifact-win-x64"), artifact.DownloadUri);
        Assert.Equal("\"sha256:artifacthash\"", artifact.ETag);
        Assert.Equal(ManifestHashHelper.ComputeStrongETag(envelope.Document), envelope.ETag);
    }

    [Fact]
    public async Task GetManifestAsync_WhenBaseUriIsInvalid_ShouldFallbackToLocalhost()
    {
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateValidCatalog() };
        var service = CreateService(repository);
        var request = new ManifestRequest { Channel = "stable", BaseUri = "not-a-valid-uri" };

        var envelope = await service.GetManifestAsync(request, CreateCustomerContext(), CancellationToken.None);

        var component = Assert.Single(envelope!.Document.Components);
        var artifact = Assert.Single(component.Artifacts);
        Assert.Equal("https://localhost:5001/api/wizard/artifacts/artifact-win-x64", artifact.DownloadUri.ToString());
    }

    [Fact]
    public async Task GetManifestAsync_WhenReleaseIsMissing_ShouldThrowNotFound()
    {
        var repository = new FakeReleaseCatalogRepository
        {
            Catalog = new CatalogDocument
            {
                ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
                {
                    ["stable"] = new ChannelPointerDocument
                    {
                        Channel = "stable",
                        ManifestReleaseId = "manifest-r1",
                        WizardBinaryReleaseId = "wizard-r1",
                    },
                },
            },
        };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.GetManifestAsync(
                new ManifestRequest { Channel = "stable" },
                CreateCustomerContext(),
                CancellationToken.None
            )
        );

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetManifestAsync_WhenArtifactReferenceIsMissing_ShouldThrowServerError()
    {
        var component = CreateComponent("agent", ["artifact-missing"]);
        var release = CreateManifestRelease("manifest-r1", [component]);
        var catalog = new CatalogDocument
        {
            ManifestReleases = new Dictionary<string, ManifestReleaseDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [release.Id] = release,
            },
            ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = new ChannelPointerDocument
                {
                    Channel = "stable",
                    ManifestReleaseId = release.Id,
                    WizardBinaryReleaseId = "wizard-r1",
                },
            },
        };
        var repository = new FakeReleaseCatalogRepository { Catalog = catalog };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.GetManifestAsync(
                new ManifestRequest { Channel = "stable" },
                CreateCustomerContext(),
                CancellationToken.None
            )
        );

        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }

    [Fact]
    public async Task GetManifestAsync_WhenChannelIsNotAllowedByPolicy_ShouldThrowForbidden()
    {
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateValidCatalog() };
        var options = new SupplyApiOptions
        {
            Customers = new Dictionary<string, CustomerPolicyOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["contoso"] = new CustomerPolicyOptions { AllowedChannels = ["beta"] },
            },
        };
        var service = CreateService(repository, options);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.GetManifestAsync(
                new ManifestRequest { Channel = "stable" },
                CreateCustomerContext(),
                CancellationToken.None
            )
        );

        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    private static WizardManifestService CreateService(
        FakeReleaseCatalogRepository repository,
        SupplyApiOptions? options = null
    ) => new(repository, Options.Create(options ?? new SupplyApiOptions()));

    private static CatalogDocument CreateValidCatalog()
    {
        var artifact = CreateArtifact("artifact-win-x64", "windows", "x64", "artifacthash");
        var component = CreateComponent("agent", [artifact.Id]);
        var release = CreateManifestRelease("manifest-r1", [component]);

        return new CatalogDocument
        {
            Artifacts = new Dictionary<string, ArtifactDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [artifact.Id] = artifact,
            },
            ManifestReleases = new Dictionary<string, ManifestReleaseDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [release.Id] = release,
            },
            ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = new ChannelPointerDocument
                {
                    Channel = "stable",
                    ManifestReleaseId = release.Id,
                    WizardBinaryReleaseId = "wizard-r1",
                },
            },
        };
    }

    private static ManifestReleaseDocument CreateManifestRelease(
        string id,
        IReadOnlyList<ManifestComponentDocument> components
    ) =>
        new()
        {
            Id = id,
            Channel = "stable",
            ReleaseVersion = "2026.02.1",
            MinWizardVersion = "1.0.0",
            Components = components,
            Dependencies =
            [
                new ManifestDependencyDocument
                {
                    Id = "redis",
                    DisplayName = "Redis",
                    DefaultPolicy = DependencyPolicy.Managed,
                    ManagedComponentId = "redis-component",
                    ProbeScheme = "tcp",
                    ProbePort = 6379,
                },
            ],
        };

    private static ManifestComponentDocument CreateComponent(string id, IReadOnlyList<string> artifactIds) =>
        new()
        {
            Id = id,
            DisplayName = "Agent",
            Version = "1.2.3",
            EnabledByDefault = true,
            DependsOnComponentIds = [],
            DependencyIds = [],
            ArtifactIds = artifactIds,
            Service = new ServiceDocument
            {
                ServiceName = "supply-agent",
                DisplayName = "Supply Agent",
                ExecutablePath = "agent.exe",
                Arguments = ["--run"],
                WorkingDirectoryPath = "C:\\Supply\\Agent",
                EnvironmentVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DOTNET_ENVIRONMENT"] = "Production",
                },
                DefaultInstallPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["windows"] = "C:\\Supply\\Agent",
                },
            },
        };

    private static ArtifactDocument CreateArtifact(string id, string os, string architecture, string sha256) =>
        new()
        {
            Id = id,
            RelativePath = $"artifacts/{id}.zip",
            FileName = $"{id}.zip",
            ContentType = "application/zip",
            SizeBytes = 1234,
            Sha256 = sha256,
            PackageType = "zip",
            Os = os,
            Architecture = architecture,
            PublishedAtUtc = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
        };

    private static CustomerContext CreateCustomerContext() => new() { CustomerId = "contoso", IsAuthenticated = true };
}
