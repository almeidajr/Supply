namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the component manifest document.
/// </summary>
public sealed record ComponentManifestDocument
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
    /// Gets or sets the artifacts.
    /// </summary>
    public IReadOnlyList<ArtifactManifestDocument> Artifacts { get; init; } = [];

    /// <summary>
    /// Gets or sets the service.
    /// </summary>
    public required ServiceManifestDocument Service { get; init; }
}
