namespace Supply.Wizard.Domain;

/// <summary>
/// Manifest entry for an edge component that can be managed by the wizard.
/// </summary>
public sealed record ComponentManifest
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

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
    public IReadOnlyList<ArtifactManifest> Artifacts { get; init; } = [];

    /// <summary>
    /// Gets or sets the service.
    /// </summary>
    public ServiceDefinition Service { get; init; } = new();
}
