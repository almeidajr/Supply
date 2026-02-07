namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to internal publish channel.
/// </summary>
public sealed record InternalPublishChannelRequest
{
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the manifest release id.
    /// </summary>
    public string? ManifestReleaseId { get; init; }

    /// <summary>
    /// Gets or sets the wizard binary release id.
    /// </summary>
    public string? WizardBinaryReleaseId { get; init; }
}
