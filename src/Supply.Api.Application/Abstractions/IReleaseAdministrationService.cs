using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

public interface IReleaseAdministrationService
{
    Task UpsertReleaseAsync(InternalUpsertReleaseRequest request, CancellationToken cancellationToken);

    Task PublishChannelAsync(
        string releaseId,
        InternalPublishChannelRequest request,
        CancellationToken cancellationToken
    );
}
