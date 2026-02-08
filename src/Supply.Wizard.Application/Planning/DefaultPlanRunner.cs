using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Planning.Internal;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Planning;

/// <summary>
/// Executes plan steps, writes journals, and applies rollback for reversible steps.
/// </summary>
public sealed class DefaultPlanRunner(
    IArtifactDownloader artifactDownloader,
    IChecksumVerifier checksumVerifier,
    IStateStore stateStore,
    IServiceManager serviceManager,
    IProcessRunner processRunner
) : IPlanRunner
{
    /// <inheritdoc />
    public async Task<PlanExecutionResult> RunAsync(
        ExecutionPlan plan,
        RunContext context,
        CancellationToken cancellationToken
    )
    {
        var stepExecutionRecords = new List<StepExecutionRecord>();
        var executedReversibleSteps = new List<IPlanStep>();
        var workingState = StateCloner.Clone(plan.InitialState);

        var stepContext = new StepContext
        {
            RunId = plan.RunId,
            Request = context.Request,
            Manifest = plan.Manifest,
            ArtifactDownloader = artifactDownloader,
            ChecksumVerifier = checksumVerifier,
            StateStore = stateStore,
            ServiceManager = serviceManager,
            ProcessRunner = processRunner,
            State = workingState,
        };

        await stateStore.AppendJournalAsync(
            context.Request.JournalFilePath,
            new JournalEntry
            {
                RunId = plan.RunId,
                EventType = "run_started",
                Message = $"Operation '{plan.Operation}' started.",
            },
            cancellationToken
        );

        try
        {
            foreach (var step in plan.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = context.DryRun
                    ? StepResult.Success($"Dry-run: {step.Name}")
                    : await step.ExecuteAsync(stepContext, cancellationToken);

                stepExecutionRecords.Add(
                    new StepExecutionRecord
                    {
                        StepId = step.Id,
                        StepName = step.Name,
                        Succeeded = result.Succeeded,
                        Message = result.Message,
                    }
                );

                await stateStore.AppendJournalAsync(
                    context.Request.JournalFilePath,
                    new JournalEntry
                    {
                        RunId = plan.RunId,
                        EventType = result.Succeeded ? "step_succeeded" : "step_failed",
                        Message = result.Message,
                        StepId = step.Id,
                    },
                    cancellationToken
                );

                if (!result.Succeeded)
                {
                    return await RollbackAsync(
                        context,
                        stepContext,
                        executedReversibleSteps,
                        stepExecutionRecords,
                        $"Step '{step.Name}' failed: {result.Message}",
                        cancellationToken
                    );
                }

                if (step.IsReversible && !context.DryRun)
                {
                    executedReversibleSteps.Add(step);
                }
            }

            if (!context.DryRun)
            {
                var completedState = workingState with { UpdatedAtUtc = DateTimeOffset.UtcNow };

                await stateStore.SaveAsync(context.Request.StateFilePath, completedState, cancellationToken);
                workingState = completedState;
            }

            await stateStore.AppendJournalAsync(
                context.Request.JournalFilePath,
                new JournalEntry
                {
                    RunId = plan.RunId,
                    EventType = "run_completed",
                    Message = "Operation completed successfully.",
                },
                cancellationToken
            );

            return new PlanExecutionResult
            {
                Succeeded = true,
                FinalState = workingState,
                StepResults = stepExecutionRecords,
                Message = context.DryRun ? "Dry-run completed successfully." : "Operation completed successfully.",
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return await RollbackAsync(
                context,
                stepContext,
                executedReversibleSteps,
                stepExecutionRecords,
                $"Execution failed: {exception.Message}",
                cancellationToken
            );
        }
    }

    private async Task<PlanExecutionResult> RollbackAsync(
        RunContext runContext,
        StepContext stepContext,
        List<IPlanStep> executedReversibleSteps,
        IReadOnlyList<StepExecutionRecord> stepExecutionRecords,
        string failureMessage,
        CancellationToken cancellationToken
    )
    {
        bool rollbackAttempted = executedReversibleSteps.Count > 0 && !runContext.DryRun;
        bool rollbackSucceeded = true;

        if (rollbackAttempted)
        {
            for (var index = executedReversibleSteps.Count - 1; index >= 0; index--)
            {
                var step = executedReversibleSteps[index];
                try
                {
                    await step.RollbackAsync(stepContext, cancellationToken);
                    await stateStore.AppendJournalAsync(
                        runContext.Request.JournalFilePath,
                        new JournalEntry
                        {
                            RunId = stepContext.RunId,
                            EventType = "rollback_succeeded",
                            Message = $"Rollback succeeded for '{step.Name}'.",
                            StepId = step.Id,
                        },
                        cancellationToken
                    );
                }
                catch (Exception rollbackException)
                {
                    rollbackSucceeded = false;
                    await stateStore.AppendJournalAsync(
                        runContext.Request.JournalFilePath,
                        new JournalEntry
                        {
                            RunId = stepContext.RunId,
                            EventType = "rollback_failed",
                            Message = $"Rollback failed for '{step.Name}': {rollbackException.Message}",
                            StepId = step.Id,
                        },
                        cancellationToken
                    );
                }
            }
        }

        await stateStore.AppendJournalAsync(
            runContext.Request.JournalFilePath,
            new JournalEntry
            {
                RunId = stepContext.RunId,
                EventType = "run_failed",
                Message = failureMessage,
            },
            cancellationToken
        );

        return new PlanExecutionResult
        {
            Succeeded = false,
            RollbackAttempted = rollbackAttempted,
            RollbackSucceeded = rollbackSucceeded,
            FinalState = StateCloner.Clone(stepContext.State),
            StepResults = stepExecutionRecords,
            Message = failureMessage,
        };
    }
}
