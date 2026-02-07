namespace Supply.Wizard.Domain;

/// <summary>
/// Dependency entry for managed or externally-provided requirements.
/// </summary>
public sealed record DependencyManifest
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public DependencyPolicy DefaultPolicy { get; init; } = DependencyPolicy.Managed;

    public string? ManagedComponentId { get; init; }
}
