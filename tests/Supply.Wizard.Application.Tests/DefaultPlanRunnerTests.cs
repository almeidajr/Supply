using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Planning;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Tests;

public sealed class DefaultPlanRunnerTests
{
    [Fact]
    public async Task RunAsync_WhenDryRun_ShouldSkipStepExecutionAndStateSave()
    {
        var stateStore = new InMemoryStateStore();
        var step = new SpyPlanStep("step.one", isReversible: true, execute: static () => StepResult.Success("ok"));
        var runner = CreateRunner(stateStore);
        var plan = CreatePlan([step], new WizardState());

        var result = await runner.RunAsync(
            plan,
            new RunContext { Request = plan.Request with { DryRun = true }, DryRun = true },
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(0, step.ExecuteCalls);
        Assert.Equal(0, stateStore.SaveCalls);
        Assert.Contains(stateStore.Journals, entry => entry.EventType == "run_completed");
    }

    [Fact]
    public async Task RunAsync_WhenStepFails_ShouldRollbackReversibleCompletedSteps()
    {
        var stateStore = new InMemoryStateStore();
        var firstStep = new SpyPlanStep(
            "step.first",
            isReversible: true,
            execute: static () => StepResult.Success("ok")
        );
        var secondStep = new SpyPlanStep(
            "step.second",
            isReversible: false,
            execute: static () => StepResult.Failure("boom")
        );
        var runner = CreateRunner(stateStore);
        var plan = CreatePlan([firstStep, secondStep], new WizardState());

        var result = await runner.RunAsync(
            plan,
            new RunContext { Request = plan.Request, DryRun = false },
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.True(result.RollbackAttempted);
        Assert.True(result.RollbackSucceeded);
        Assert.Equal(1, firstStep.ExecuteCalls);
        Assert.Equal(1, firstStep.RollbackCalls);
        Assert.Equal(1, secondStep.ExecuteCalls);
        Assert.Contains(stateStore.Journals, entry => entry.EventType == "rollback_succeeded");
    }

    [Fact]
    public async Task RunAsync_WhenRollbackThrows_ShouldReportRollbackFailed()
    {
        var stateStore = new InMemoryStateStore();
        var firstStep = new SpyPlanStep(
            "step.first",
            isReversible: true,
            execute: static () => StepResult.Success("ok"),
            rollback: static () => Task.FromException(new InvalidOperationException("rollback failed"))
        );
        var secondStep = new SpyPlanStep(
            "step.second",
            isReversible: false,
            execute: static () => StepResult.Failure("boom")
        );
        var runner = CreateRunner(stateStore);
        var plan = CreatePlan([firstStep, secondStep], new WizardState());

        var result = await runner.RunAsync(
            plan,
            new RunContext { Request = plan.Request, DryRun = false },
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.True(result.RollbackAttempted);
        Assert.False(result.RollbackSucceeded);
        Assert.Equal(1, firstStep.RollbackCalls);
        Assert.Contains(stateStore.Journals, entry => entry.EventType == "rollback_failed");
    }

    private static DefaultPlanRunner CreateRunner(IStateStore stateStore) =>
        new(
            new NoOpArtifactDownloader(),
            new NoOpChecksumVerifier(),
            stateStore,
            new NoOpServiceManager(),
            new NoOpProcessRunner()
        );

    private static ExecutionPlan CreatePlan(IReadOnlyList<IPlanStep> steps, WizardState initialState)
    {
        var request = new WizardRequest
        {
            Operation = OperationKind.Install,
            ApiBaseUri = new Uri("https://localhost:5001"),
            CacheDirectoryPath = Path.Combine(Path.GetTempPath(), "supply-tests", "cache"),
            StateFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "state.json"),
            JournalFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "journal.jsonl"),
        };

        return new ExecutionPlan
        {
            RunId = Guid.NewGuid(),
            Operation = OperationKind.Install,
            Request = request,
            Manifest = new ManifestDocument { ManifestVersion = "test", PublishedAtUtc = DateTimeOffset.UtcNow },
            InitialState = initialState,
            TargetState = initialState,
            Steps = steps,
        };
    }
}
