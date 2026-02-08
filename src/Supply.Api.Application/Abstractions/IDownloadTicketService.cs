using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

public interface IDownloadTicketService
{
    Task<DownloadTicketResponse> CreateTicketsAsync(
        DownloadTicketRequest request,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
