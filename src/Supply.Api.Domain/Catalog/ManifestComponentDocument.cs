namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the manifest component document.
/// </summary>
public sealed record ManifestComponentDocument
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets or sets the enabled by default.
    /// </summary>
    public bool EnabledByDefault { get; init; } = true;

    /// <summary>
    /// Gets or sets the depends on component ids.
    /// </summary>
    public IReadOnlyList<string> DependsOnComponentIds { get; init; } = [];

    /// <summary>
    /// Gets or sets the dependency ids.
    /// </summary>
    public IReadOnlyList<string> DependencyIds { get; init; } = [];

    /// <summary>
    /// Gets or sets the artifact ids.
    /// </summary>
    public IReadOnlyList<string> ArtifactIds { get; init; } = [];

    /// <summary>
    /// Gets or sets the service.
    /// </summary>
    public required ServiceDocument Service { get; init; }
}
