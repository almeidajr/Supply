namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to download ticket.
/// </summary>
public sealed record DownloadTicketRequest
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    public IReadOnlyList<DownloadTicketItemRequest> Items { get; init; } = [];

    /// <summary>
    /// Gets or sets the time to live seconds.
    /// </summary>
    public int TimeToLiveSeconds { get; init; } = 300;
}
