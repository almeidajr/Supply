namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the download ticket link document.
/// </summary>
public sealed record DownloadTicketLinkDocument
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the url.
    /// </summary>
    public required Uri Url { get; init; }
}
