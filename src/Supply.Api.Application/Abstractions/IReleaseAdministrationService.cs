using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Handles internal release administration operations.
/// </summary>
public interface IReleaseAdministrationService
{
    /// <summary>
    /// Inserts or updates a release definition in the catalog.
    /// </summary>
    /// <param name="request">Release upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertReleaseAsync(InternalUpsertReleaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes release identifiers to a channel pointer.
    /// </summary>
    /// <param name="releaseId">Release identifier from route context.</param>
    /// <param name="request">Publish channel request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishChannelAsync(
        string releaseId,
        InternalPublishChannelRequest request,
        CancellationToken cancellationToken
    );
}
