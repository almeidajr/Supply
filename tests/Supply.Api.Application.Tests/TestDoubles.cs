using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Tests;

internal sealed class FakeReleaseCatalogRepository : IReleaseCatalogRepository
{
    public CatalogDocument Catalog { get; set; } = new();

    public List<UpsertReleaseRequest> UpsertRequests { get; } = [];

    public List<PublishReleaseRequest> PublishRequests { get; } = [];

    public Task<CatalogDocument> GetCatalogAsync(CancellationToken cancellationToken) => Task.FromResult(Catalog);

    public Task UpsertReleaseAsync(UpsertReleaseRequest request, CancellationToken cancellationToken)
    {
        UpsertRequests.Add(request);
        return Task.CompletedTask;
    }

    public Task PublishChannelPointerAsync(PublishReleaseRequest request, CancellationToken cancellationToken)
    {
        PublishRequests.Add(request);
        return Task.CompletedTask;
    }
}

internal sealed class FakeArtifactStorage : IArtifactStorage
{
    public HashSet<string> ExistingPaths { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ArtifactFileMetadata?> MetadataByPath { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Stream> StreamsByPath { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken) =>
        Task.FromResult(ExistingPaths.Contains(relativePath));

    public Task<ArtifactFileMetadata?> GetMetadataAsync(string relativePath, CancellationToken cancellationToken)
    {
        MetadataByPath.TryGetValue(relativePath, out var metadata);
        return Task.FromResult(metadata);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (!StreamsByPath.TryGetValue(relativePath, out var stream))
        {
            throw new InvalidOperationException($"No stream configured for '{relativePath}'.");
        }

        return Task.FromResult(stream);
    }
}

internal sealed class FakeWizardDistributionService : IWizardDistributionService
{
    private readonly Dictionary<string, WizardBinaryLatestDocument?> _latestByKey = new(
        StringComparer.OrdinalIgnoreCase
    );

    public List<(
        string Channel,
        string OperatingSystem,
        string Architecture,
        string? BaseUri
    )> LatestRequests { get; } = [];

    public void SetLatestWizardBinary(
        string channel,
        string operatingSystem,
        string architecture,
        WizardBinaryLatestDocument? response
    ) => _latestByKey[BuildKey(channel, operatingSystem, architecture)] = response;

    public Task<WizardBinaryLatestDocument?> GetLatestWizardBinaryAsync(
        string channel,
        string operatingSystem,
        string architecture,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        LatestRequests.Add((channel, operatingSystem, architecture, baseUri));
        _latestByKey.TryGetValue(BuildKey(channel, operatingSystem, architecture), out var response);
        return Task.FromResult(response);
    }

    public Task<ArtifactDownloadResult?> OpenWizardBinaryAsync(
        string version,
        string operatingSystem,
        string architecture,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    ) => Task.FromResult<ArtifactDownloadResult?>(null);

    public Task<ArtifactDownloadResult?> OpenArtifactAsync(
        string artifactId,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    ) => Task.FromResult<ArtifactDownloadResult?>(null);

    private static string BuildKey(string channel, string operatingSystem, string architecture) =>
        $"{channel}/{operatingSystem}/{architecture}";
}
