namespace Supply.Wizard.Domain;

/// <summary>
/// Local persisted state used for idempotency and update diff calculations.
/// </summary>
public sealed record WizardState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public DateTimeOffset UpdatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public string LastManifestVersion { get; init; } = string.Empty;

    public Dictionary<string, InstalledComponentState> Components { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ExternalDependencyState> ExternalDependencies { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
