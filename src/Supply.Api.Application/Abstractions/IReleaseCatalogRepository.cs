using Supply.Api.Domain.Catalog;

namespace Supply.Api.Application.Abstractions;

public interface IReleaseCatalogRepository
{
    Task<CatalogDocument> GetCatalogAsync(CancellationToken cancellationToken);

    Task UpsertReleaseAsync(UpsertReleaseRequest request, CancellationToken cancellationToken);

    Task PublishChannelPointerAsync(PublishReleaseRequest request, CancellationToken cancellationToken);
}

public sealed record UpsertReleaseRequest
{
    public ManifestReleaseDocument? ManifestRelease { get; init; }

    public WizardBinaryReleaseDocument? WizardBinaryRelease { get; init; }

    public IReadOnlyList<ArtifactDocument> Artifacts { get; init; } = [];
}

public sealed record PublishReleaseRequest
{
    public required string Channel { get; init; }

    public string? ManifestReleaseId { get; init; }

    public string? WizardBinaryReleaseId { get; init; }
}
