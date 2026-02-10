using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to download ticket item.
/// </summary>
public sealed record DownloadTicketItemRequest
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    [Description("Ticket item type. Expected values are 'artifact' or 'wizardBinary'.")]
    [Required]
    [MinLength(1)]
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Description("Item identifier, such as artifactId or wizard version.")]
    [Required]
    [MinLength(1)]
    public required string Id { get; init; }
}
