using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Supply.Api.Application.Abstractions;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Tests;

public sealed class WizardDistributionServiceTests
{
    [Fact]
    public async Task GetLatestWizardBinaryAsync_WhenReleaseAndArtifactExist_ShouldReturnDocument()
    {
        var artifact = CreateArtifact("wizard-win-x64", "windows", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithWizardRelease(artifact) };
        var service = CreateService(repository, new FakeArtifactStorage());

        var result = await service.GetLatestWizardBinaryAsync(
            "stable",
            "windows",
            "x64",
            "https://api.example.com/",
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal("stable", result.Channel);
        Assert.Equal("1.2.3", result.Version);
        Assert.Equal("windows", result.Os);
        Assert.Equal("x64", result.Architecture);
        Assert.Equal(new Uri("https://api.example.com/api/wizard/binaries/1.2.3/windows/x64"), result.DownloadUri);
        Assert.Equal("\"sha256:wizardhash\"", result.ETag);
    }

    [Fact]
    public async Task GetLatestWizardBinaryAsync_WhenReleaseIsMissing_ShouldReturnNull()
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
                        WizardBinaryReleaseId = "wizard-missing",
                    },
                },
            },
        };
        var service = CreateService(repository, new FakeArtifactStorage());

        var result = await service.GetLatestWizardBinaryAsync(
            "stable",
            "windows",
            "x64",
            null,
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestWizardBinaryAsync_WhenPlatformArtifactIsMissing_ShouldThrowNotFound()
    {
        var artifact = CreateArtifact("wizard-linux-x64", "linux", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithWizardRelease(artifact) };
        var service = CreateService(repository, new FakeArtifactStorage());

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.GetLatestWizardBinaryAsync(
                "stable",
                "windows",
                "x64",
                null,
                CreateCustomerContext(),
                CancellationToken.None
            )
        );

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task OpenWizardBinaryAsync_WhenVersionDoesNotExist_ShouldReturnNull()
    {
        var artifact = CreateArtifact("wizard-win-x64", "windows", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithWizardRelease(artifact) };
        var service = CreateService(repository, new FakeArtifactStorage());

        var result = await service.OpenWizardBinaryAsync(
            "9.9.9",
            "windows",
            "x64",
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task OpenArtifactAsync_WhenArtifactIsNotInCatalog_ShouldReturnNull()
    {
        var repository = new FakeReleaseCatalogRepository { Catalog = new CatalogDocument() };
        var service = CreateService(repository, new FakeArtifactStorage());

        var result = await service.OpenArtifactAsync(
            "artifact-missing",
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task OpenArtifactAsync_WhenContentIsMissing_ShouldThrowNotFound()
    {
        var artifact = CreateArtifact("wizard-win-x64", "windows", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithArtifactOnly(artifact) };
        var storage = new FakeArtifactStorage();
        var service = CreateService(repository, storage);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.OpenArtifactAsync(artifact.Id, CreateCustomerContext(), CancellationToken.None)
        );

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task OpenArtifactAsync_WhenMetadataIsMissing_ShouldThrowNotFound()
    {
        var artifact = CreateArtifact("wizard-win-x64", "windows", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithArtifactOnly(artifact) };
        var storage = new FakeArtifactStorage();
        storage.ExistingPaths.Add(artifact.RelativePath);
        var service = CreateService(repository, storage);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            service.OpenArtifactAsync(artifact.Id, CreateCustomerContext(), CancellationToken.None)
        );

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task OpenArtifactAsync_WhenArtifactExists_ShouldReturnDownloadResult()
    {
        var artifact = CreateArtifact("wizard-win-x64", "windows", "x64", "wizardhash");
        var repository = new FakeReleaseCatalogRepository { Catalog = CreateCatalogWithArtifactOnly(artifact) };
        var stream = new MemoryStream([1, 2, 3]);
        var timestamp = new DateTimeOffset(2026, 2, 3, 1, 2, 3, TimeSpan.Zero);
        var storage = new FakeArtifactStorage();
        storage.ExistingPaths.Add(artifact.RelativePath);
        storage.MetadataByPath[artifact.RelativePath] = new ArtifactFileMetadata
        {
            SizeBytes = 3,
            LastWriteAtUtc = timestamp,
        };
        storage.StreamsByPath[artifact.RelativePath] = stream;
        var service = CreateService(repository, storage);

        var result = await service.OpenArtifactAsync(artifact.Id, CreateCustomerContext(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Same(stream, result.ContentStream);
        Assert.Equal("application/zip", result.ContentType);
        Assert.Equal("wizard-win-x64.zip", result.FileName);
        Assert.Equal(3, result.SizeBytes);
        Assert.Equal("\"sha256:wizardhash\"", result.ETag);
        Assert.Equal(timestamp, result.LastModifiedUtc);
    }

    private static WizardDistributionService CreateService(
        FakeReleaseCatalogRepository repository,
        FakeArtifactStorage artifactStorage,
        SupplyApiOptions? options = null
    ) => new(repository, artifactStorage, Options.Create(options ?? new SupplyApiOptions()));

    private static CatalogDocument CreateCatalogWithWizardRelease(ArtifactDocument artifact)
    {
        var release = new WizardBinaryReleaseDocument
        {
            Id = "wizard-r1",
            Channel = "stable",
            Version = "1.2.3",
            ArtifactIds = [artifact.Id],
        };

        return new CatalogDocument
        {
            Artifacts = new Dictionary<string, ArtifactDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [artifact.Id] = artifact,
            },
            WizardBinaryReleases = new Dictionary<string, WizardBinaryReleaseDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [release.Id] = release,
            },
            ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = new ChannelPointerDocument
                {
                    Channel = "stable",
                    ManifestReleaseId = "manifest-r1",
                    WizardBinaryReleaseId = release.Id,
                },
            },
        };
    }

    private static CatalogDocument CreateCatalogWithArtifactOnly(ArtifactDocument artifact) =>
        new()
        {
            Artifacts = new Dictionary<string, ArtifactDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [artifact.Id] = artifact,
            },
        };

    private static ArtifactDocument CreateArtifact(string id, string os, string architecture, string sha256) =>
        new()
        {
            Id = id,
            RelativePath = $"artifacts/{id}.zip",
            FileName = $"{id}.zip",
            ContentType = "application/zip",
            SizeBytes = 200,
            Sha256 = sha256,
            PackageType = "zip",
            Os = os,
            Architecture = architecture,
            PublishedAtUtc = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
        };

    private static CustomerContext CreateCustomerContext() => new() { CustomerId = "contoso", IsAuthenticated = true };
}
