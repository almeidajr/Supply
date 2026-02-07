namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to download ticket item.
/// </summary>
public sealed record DownloadTicketItemRequest
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }
}
