namespace Supply.Wizard.Domain;

/// <summary>
/// Manifest payload returned by Supply.Api for edge deployment planning.
/// </summary>
public sealed record ManifestDocument
{
    /// <summary>
    /// Gets or sets the manifest version.
    /// </summary>
    public string ManifestVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the published at utc.
    /// </summary>
    public DateTimeOffset PublishedAtUtc { get; init; }

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    public IReadOnlyList<ComponentManifest> Components { get; init; } = [];

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public IReadOnlyList<DependencyManifest> Dependencies { get; init; } = [];
}
