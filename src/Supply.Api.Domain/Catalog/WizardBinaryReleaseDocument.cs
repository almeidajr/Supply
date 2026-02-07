namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the wizard binary release document.
/// </summary>
public sealed record WizardBinaryReleaseDocument
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets or sets the artifact ids.
    /// </summary>
    public IReadOnlyList<string> ArtifactIds { get; init; } = [];
}
