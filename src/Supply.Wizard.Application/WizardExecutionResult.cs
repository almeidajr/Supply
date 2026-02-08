using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// End-to-end wizard execution result.
/// </summary>
public sealed record WizardExecutionResult
{
    /// <summary>
    /// Gets the wizard process exit code.
    /// </summary>
    public required WizardExitCode ExitCode { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public required bool Succeeded { get; init; }

    /// <summary>
    /// Gets a value indicating whether execution ran in dry-run mode.
    /// </summary>
    public required bool DryRun { get; init; }

    /// <summary>
    /// Gets a summary message for the execution outcome.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the generated execution plan, when available.
    /// </summary>
    public ExecutionPlan? Plan { get; init; }

    /// <summary>
    /// Gets plan execution details, when steps were executed.
    /// </summary>
    public PlanExecutionResult? RunResult { get; init; }
}
