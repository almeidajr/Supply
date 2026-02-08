using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Per-execution context used by the plan runner.
/// </summary>
public sealed record RunContext
{
    /// <summary>
    /// Gets the original wizard request.
    /// </summary>
    public required WizardRequest Request { get; init; }

    /// <summary>
    /// Gets a value indicating whether execution should run in dry-run mode.
    /// </summary>
    public bool DryRun { get; init; }
}
