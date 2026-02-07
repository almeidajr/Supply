using Supply.Api.Domain.Contracts;

namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the manifest dependency document.
/// </summary>
public sealed record ManifestDependencyDocument
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
    /// Gets or sets the default policy.
    /// </summary>
    public DependencyPolicy DefaultPolicy { get; init; }

    /// <summary>
    /// Gets or sets the managed component id.
    /// </summary>
    public string? ManagedComponentId { get; init; }

    /// <summary>
    /// Gets or sets the probe scheme.
    /// </summary>
    public string? ProbeScheme { get; init; }

    /// <summary>
    /// Gets or sets the probe port.
    /// </summary>
    public int? ProbePort { get; init; }
}
