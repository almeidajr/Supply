namespace Supply.Wizard.Domain;

/// <summary>
/// Unified request model shared by CLI and interactive flows.
/// </summary>
public sealed record WizardRequest
{
    public required OperationKind Operation { get; init; }

    public required Uri ApiBaseUri { get; init; }

    public string Channel { get; init; } = "stable";

    public bool DryRun { get; init; }

    public bool AutoApprove { get; init; }

    public bool NonInteractive { get; init; }

    public bool PurgeData { get; init; }

    public string CacheDirectoryPath { get; init; } = string.Empty;

    public string StateFilePath { get; init; } = string.Empty;

    public string JournalFilePath { get; init; } = string.Empty;

    public IReadOnlyList<string> TargetComponentIds { get; init; } = [];

    public IReadOnlyDictionary<string, DependencyPolicy> DependencyPolicies { get; init; } =
        new Dictionary<string, DependencyPolicy>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Uri> ExternalDependencyEndpoints { get; init; } =
        new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);

    public WizardAuthOptions Authentication { get; init; } = new();

    public WizardTlsOptions Tls { get; init; } = new();
}
