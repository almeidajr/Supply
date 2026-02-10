using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to download ticket.
/// </summary>
public sealed record DownloadTicketRequest
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    [Description("Requested wizard binaries or artifacts that need signed download links.")]
    [Required]
    [MinLength(1)]
    public IReadOnlyList<DownloadTicketItemRequest> Items { get; init; } = [];

    /// <summary>
    /// Gets or sets the time to live seconds.
    /// </summary>
    [Description("Lifetime of each generated download ticket in seconds.")]
    [Range(1, 86_400)]
    public int TimeToLiveSeconds { get; init; } = 300;
}
