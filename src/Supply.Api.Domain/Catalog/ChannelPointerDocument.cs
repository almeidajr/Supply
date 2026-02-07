namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the channel pointer document.
/// </summary>
public sealed record ChannelPointerDocument
{
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the manifest release id.
    /// </summary>
    public required string ManifestReleaseId { get; init; }

    /// <summary>
    /// Gets or sets the wizard binary release id.
    /// </summary>
    public required string WizardBinaryReleaseId { get; init; }
}
