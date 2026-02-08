using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Tests;

internal sealed class InMemoryStateStore : IStateStore
{
    public int SaveCalls { get; private set; }

    public List<JournalEntry> Journals { get; } = [];

    public WizardState State { get; private set; } = new();

    public Task<WizardState> LoadAsync(string stateFilePath, CancellationToken cancellationToken)
    {
        return Task.FromResult(State);
    }

    public Task SaveAsync(string stateFilePath, WizardState state, CancellationToken cancellationToken)
    {
        SaveCalls++;
        State = state;
        return Task.CompletedTask;
    }

    public Task AppendJournalAsync(string journalFilePath, JournalEntry entry, CancellationToken cancellationToken)
    {
        Journals.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class NoOpArtifactDownloader : IArtifactDownloader
{
    public Task<ArtifactDownloadResult> DownloadAsync(
        ArtifactManifest artifact,
        DownloadContext context,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(new ArtifactDownloadResult { FilePath = string.Empty, ReusedCachedFile = true });
    }
}

internal sealed class NoOpChecksumVerifier : IChecksumVerifier
{
    public Task VerifySha256Async(string filePath, string expectedSha256, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal sealed class NoOpServiceManager : IServiceManager
{
    public Task<bool> ExistsAsync(string serviceName, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task CreateOrUpdateAsync(ServiceDefinition definition, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StartAsync(string serviceName, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(string serviceName, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(string serviceName, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken) =>
        Task.FromResult(ServiceStatus.Unknown);
}

internal sealed class NoOpProcessRunner : IProcessRunner
{
    public Task<ProcessResult> RunAsync(ProcessSpec spec, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProcessResult { ExitCode = 0 });
    }
}

internal sealed class SpyPlanStep(string id, bool isReversible, Func<StepResult> execute, Func<Task>? rollback = null)
    : IPlanStep
{
    private readonly Func<StepResult> _executeAction = execute;
    private readonly Func<Task> _rollbackAction = rollback ?? (() => Task.CompletedTask);

    public int ExecuteCalls { get; private set; }

    public int RollbackCalls { get; private set; }

    public string Id { get; } = id;

    public string Name => Id;

    public bool IsReversible { get; } = isReversible;

    public Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        ExecuteCalls++;
        return Task.FromResult(_executeAction());
    }

    public Task RollbackAsync(StepContext context, CancellationToken cancellationToken)
    {
        RollbackCalls++;
        return _rollbackAction();
    }
}
