namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Executes plan steps and handles rollback.
/// </summary>
public interface IPlanRunner
{
    /// <summary>
    /// Executes a plan and returns execution details, including rollback status when applicable.
    /// </summary>
    /// <param name="plan">Plan to execute.</param>
    /// <param name="context">Runtime context for the plan execution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The plan execution result.</returns>
    Task<PlanExecutionResult> RunAsync(ExecutionPlan plan, RunContext context, CancellationToken cancellationToken);
}
