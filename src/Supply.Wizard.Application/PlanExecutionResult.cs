using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Plan runner output, including rollback status.
/// </summary>
public sealed record PlanExecutionResult
{
    /// <summary>
    /// Gets a value indicating whether the plan completed successfully.
    /// </summary>
    public required bool Succeeded { get; init; }

    /// <summary>
    /// Gets a value indicating whether rollback was attempted.
    /// </summary>
    public bool RollbackAttempted { get; init; }

    /// <summary>
    /// Gets a value indicating whether rollback succeeded.
    /// </summary>
    public bool RollbackSucceeded { get; init; }

    /// <summary>
    /// Gets the final state snapshot after execution or rollback.
    /// </summary>
    public WizardState FinalState { get; init; } = new();

    /// <summary>
    /// Gets per-step execution results.
    /// </summary>
    public IReadOnlyList<StepExecutionRecord> StepResults { get; init; } = [];

    /// <summary>
    /// Gets a summary message for the overall run outcome.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
