using Supply.Api.Application.Services;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Tests;

public sealed class ReleaseAdministrationServiceTests
{
    [Fact]
    public async Task UpsertReleaseAsync_WhenCalled_ShouldForwardMappedRequestToRepository()
    {
        var repository = new FakeReleaseCatalogRepository();
        var service = new ReleaseAdministrationService(repository);
        var manifestRelease = new ManifestReleaseDocument
        {
            Id = "manifest-r1",
            Channel = "stable",
            ReleaseVersion = "2026.02.1",
            MinWizardVersion = "1.0.0",
        };
        var wizardBinaryRelease = new WizardBinaryReleaseDocument
        {
            Id = "wizard-r1",
            Channel = "stable",
            Version = "1.2.3",
            ArtifactIds = ["wizard-win-x64"],
        };
        var artifact = CreateArtifact("wizard-win-x64");
        var request = new InternalUpsertReleaseRequest
        {
            ManifestRelease = manifestRelease,
            WizardBinaryRelease = wizardBinaryRelease,
            Artifacts = [artifact],
        };

        await service.UpsertReleaseAsync(request, CancellationToken.None);

        var forwarded = Assert.Single(repository.UpsertRequests);
        Assert.Same(manifestRelease, forwarded.ManifestRelease);
        Assert.Same(wizardBinaryRelease, forwarded.WizardBinaryRelease);
        Assert.Same(artifact, Assert.Single(forwarded.Artifacts));
    }

    [Fact]
    public async Task PublishChannelAsync_WhenCalled_ShouldForwardPublishRequestToRepository()
    {
        var repository = new FakeReleaseCatalogRepository();
        var service = new ReleaseAdministrationService(repository);
        var request = new InternalPublishChannelRequest
        {
            Channel = "stable",
            ManifestReleaseId = "manifest-r1",
            WizardBinaryReleaseId = "wizard-r1",
        };

        await service.PublishChannelAsync("release-from-route", request, CancellationToken.None);

        var forwarded = Assert.Single(repository.PublishRequests);
        Assert.Equal("stable", forwarded.Channel);
        Assert.Equal("manifest-r1", forwarded.ManifestReleaseId);
        Assert.Equal("wizard-r1", forwarded.WizardBinaryReleaseId);
    }

    private static ArtifactDocument CreateArtifact(string id) =>
        new()
        {
            Id = id,
            RelativePath = $"artifacts/{id}.zip",
            FileName = $"{id}.zip",
            ContentType = "application/zip",
            SizeBytes = 128,
            Sha256 = "abc123",
            PackageType = "zip",
            Os = "windows",
            Architecture = "x64",
            PublishedAtUtc = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
        };
}
