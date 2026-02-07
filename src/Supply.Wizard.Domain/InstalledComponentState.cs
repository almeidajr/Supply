namespace Supply.Wizard.Domain;

/// <summary>
/// Installed component metadata tracked by local state.
/// </summary>
public sealed record InstalledComponentState
{
    /// <summary>
    /// Gets or sets the component id.
    /// </summary>
    public string ComponentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the installed path.
    /// </summary>
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the installed at utc.
    /// </summary>
    public DateTimeOffset InstalledAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
