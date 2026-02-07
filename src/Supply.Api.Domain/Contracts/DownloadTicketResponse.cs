namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a response for download ticket.
/// </summary>
public sealed record DownloadTicketResponse
{
    /// <summary>
    /// Gets or sets the expires at utc.
    /// </summary>
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    /// <summary>
    /// Gets or sets the links.
    /// </summary>
    public IReadOnlyList<DownloadTicketLinkDocument> Links { get; init; } = [];
}
