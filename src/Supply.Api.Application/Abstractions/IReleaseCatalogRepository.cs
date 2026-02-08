using Supply.Api.Domain.Catalog;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Provides persistence for release catalog reads and writes.
/// </summary>
public interface IReleaseCatalogRepository
{
    /// <summary>
    /// Loads the current release catalog.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Catalog document.</returns>
    Task<CatalogDocument> GetCatalogAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Inserts or updates release entities in the catalog.
    /// </summary>
    /// <param name="request">Upsert request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertReleaseAsync(UpsertReleaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes channel pointers for manifest and wizard binary releases.
    /// </summary>
    /// <param name="request">Publish request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishChannelPointerAsync(PublishReleaseRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Request payload used to upsert release catalog entities.
/// </summary>
public sealed record UpsertReleaseRequest
{
    /// <summary>
    /// Gets the manifest release to insert or update.
    /// </summary>
    public ManifestReleaseDocument? ManifestRelease { get; init; }

    /// <summary>
    /// Gets the wizard binary release to insert or update.
    /// </summary>
    public WizardBinaryReleaseDocument? WizardBinaryRelease { get; init; }

    /// <summary>
    /// Gets the artifacts to insert or update.
    /// </summary>
    public IReadOnlyList<ArtifactDocument> Artifacts { get; init; } = [];
}

/// <summary>
/// Request payload used to publish a channel pointer.
/// </summary>
public sealed record PublishReleaseRequest
{
    /// <summary>
    /// Gets the channel name.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets the manifest release identifier to publish.
    /// </summary>
    public string? ManifestReleaseId { get; init; }

    /// <summary>
    /// Gets the wizard binary release identifier to publish.
    /// </summary>
    public string? WizardBinaryReleaseId { get; init; }
}
