namespace Supply.Wizard.Domain;

/// <summary>
/// Dependency entry for managed or externally-provided requirements.
/// </summary>
public sealed record DependencyManifest
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
    /// Gets or sets the default policy.
    /// </summary>
    public DependencyPolicy DefaultPolicy { get; init; } = DependencyPolicy.Managed;

    /// <summary>
    /// Gets or sets the managed component id.
    /// </summary>
    public string? ManagedComponentId { get; init; }
}
