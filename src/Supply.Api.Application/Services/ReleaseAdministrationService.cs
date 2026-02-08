using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Services;

/// <summary>
/// Implements internal release administration use cases.
/// </summary>
public sealed class ReleaseAdministrationService(IReleaseCatalogRepository releaseCatalogRepository)
    : IReleaseAdministrationService
{
    /// <summary>
    /// Inserts or updates release definitions in the catalog.
    /// </summary>
    /// <param name="request">Internal upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task UpsertReleaseAsync(InternalUpsertReleaseRequest request, CancellationToken cancellationToken)
    {
        return releaseCatalogRepository.UpsertReleaseAsync(
            new UpsertReleaseRequest
            {
                ManifestRelease = request.ManifestRelease,
                WizardBinaryRelease = request.WizardBinaryRelease,
                Artifacts = request.Artifacts,
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Publishes channel pointers for a release.
    /// </summary>
    /// <param name="releaseId">Release identifier from route context.</param>
    /// <param name="request">Publish request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task PublishChannelAsync(
        string releaseId,
        InternalPublishChannelRequest request,
        CancellationToken cancellationToken
    )
    {
        _ = releaseId;
        return releaseCatalogRepository.PublishChannelPointerAsync(
            new PublishReleaseRequest
            {
                Channel = request.Channel,
                ManifestReleaseId = request.ManifestReleaseId,
                WizardBinaryReleaseId = request.WizardBinaryReleaseId,
            },
            cancellationToken
        );
    }
}
