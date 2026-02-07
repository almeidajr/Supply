namespace Supply.Wizard.Domain;

/// <summary>
/// Installed component metadata tracked by local state.
/// </summary>
public sealed record InstalledComponentState
{
    public string ComponentId { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string InstalledPath { get; init; } = string.Empty;

    public string ServiceName { get; init; } = string.Empty;

    public DateTimeOffset InstalledAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
