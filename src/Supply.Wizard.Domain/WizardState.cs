namespace Supply.Wizard.Domain;

/// <summary>
/// Local persisted state used for idempotency and update diff calculations.
/// </summary>
public sealed record WizardState
{
    /// <summary>
    /// Gets the current schema version.
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the last manifest version.
    /// </summary>
    public string LastManifestVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets installed components keyed by component identifier.
    /// </summary>
    public Dictionary<string, InstalledComponentState> Components { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets external dependency states keyed by dependency identifier.
    /// </summary>
    public Dictionary<string, ExternalDependencyState> ExternalDependencies { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
