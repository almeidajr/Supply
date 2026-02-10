using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to internal publish channel.
/// </summary>
public sealed record InternalPublishChannelRequest
{
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    [Description("Release channel name to publish.")]
    [Required]
    [MinLength(1)]
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the manifest release id.
    /// </summary>
    [Description("Manifest release identifier to assign to the channel.")]
    public string? ManifestReleaseId { get; init; }

    /// <summary>
    /// Gets or sets the wizard binary release id.
    /// </summary>
    [Description("Wizard binary release identifier to assign to the channel.")]
    public string? WizardBinaryReleaseId { get; init; }
}
