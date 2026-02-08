namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Atomic step executed by the plan runner.
/// </summary>
public interface IPlanStep
{
    /// <summary>
    /// Gets a stable step identifier used for journaling and diagnostics.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets a human-readable step name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this step supports rollback.
    /// </summary>
    bool IsReversible { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="context">Execution context shared by all steps.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The step execution result.</returns>
    Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back this step after a later failure.
    /// </summary>
    /// <param name="context">Execution context shared by all steps.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when rollback finishes.</returns>
    Task RollbackAsync(StepContext context, CancellationToken cancellationToken);
}
