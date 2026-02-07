namespace Supply.Wizard.Domain;

/// <summary>
/// Manifest entry for an edge component that can be managed by the wizard.
/// </summary>
public sealed record ComponentManifest
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public bool EnabledByDefault { get; init; } = true;

    public IReadOnlyList<string> DependsOnComponentIds { get; init; } = [];

    public IReadOnlyList<string> DependencyIds { get; init; } = [];

    public IReadOnlyList<ArtifactManifest> Artifacts { get; init; } = [];

    public ServiceDefinition Service { get; init; } = new();
}
