namespace Supply.Wizard.Domain;

/// <summary>
/// Unified request model shared by CLI and interactive flows.
/// </summary>
public sealed record WizardRequest
{
    /// <summary>
    /// Gets or sets the operation.
    /// </summary>
    public required OperationKind Operation { get; init; }

    /// <summary>
    /// Gets or sets the api base uri.
    /// </summary>
    public required Uri ApiBaseUri { get; init; }

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public string Channel { get; init; } = "stable";

    /// <summary>
    /// Gets or sets the dry run.
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Gets or sets the auto approve.
    /// </summary>
    public bool AutoApprove { get; init; }

    /// <summary>
    /// Gets or sets the non interactive.
    /// </summary>
    public bool NonInteractive { get; init; }

    /// <summary>
    /// Gets or sets the purge data.
    /// </summary>
    public bool PurgeData { get; init; }

    /// <summary>
    /// Gets or sets the cache directory path.
    /// </summary>
    public string CacheDirectoryPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the state file path.
    /// </summary>
    public string StateFilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the journal file path.
    /// </summary>
    public string JournalFilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the target component ids.
    /// </summary>
    public IReadOnlyList<string> TargetComponentIds { get; init; } = [];

    /// <summary>
    /// Gets or sets dependency policy overrides keyed by dependency identifier.
    /// </summary>
    public IReadOnlyDictionary<string, DependencyPolicy> DependencyPolicies { get; init; } =
        new Dictionary<string, DependencyPolicy>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets external dependency endpoint overrides keyed by dependency identifier.
    /// </summary>
    public IReadOnlyDictionary<string, Uri> ExternalDependencyEndpoints { get; init; } =
        new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the authentication.
    /// </summary>
    public WizardAuthOptions Authentication { get; init; } = new();

    /// <summary>
    /// Gets or sets the tls.
    /// </summary>
    public WizardTlsOptions Tls { get; init; } = new();
}
