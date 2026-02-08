using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Creates time-limited download ticket links for wizard clients.
/// </summary>
public interface IDownloadTicketService
{
    /// <summary>
    /// Creates download ticket links for the requested items.
    /// </summary>
    /// <param name="request">Download ticket request payload.</param>
    /// <param name="baseUri">Optional absolute base URI used to construct returned links.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Download ticket response with expiration and links.</returns>
    Task<DownloadTicketResponse> CreateTicketsAsync(
        DownloadTicketRequest request,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
