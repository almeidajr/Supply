using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Services;

public sealed class ReleaseAdministrationService(IReleaseCatalogRepository releaseCatalogRepository)
    : IReleaseAdministrationService
{
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
