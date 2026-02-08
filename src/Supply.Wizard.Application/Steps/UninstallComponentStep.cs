using Supply.Wizard.Application.Abstractions;

namespace Supply.Wizard.Application.Steps;

/// <summary>
/// Stops and removes a managed component.
/// </summary>
public sealed class UninstallComponentStep(string componentId, bool purgeData) : IPlanStep
{
    /// <inheritdoc />
    public string Id => $"component.remove.{componentId}";

    /// <inheritdoc />
    public string Name => $"Uninstall '{componentId}'";

    /// <inheritdoc />
    public bool IsReversible => false;

    /// <inheritdoc />
    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        if (context.Request.DryRun)
        {
            return StepResult.Success($"Dry-run: skipped uninstall for {componentId}.");
        }

        if (!context.State.Components.TryGetValue(componentId, out var componentState))
        {
            return StepResult.Success($"Component '{componentId}' is not installed.");
        }

        if (
            !string.IsNullOrWhiteSpace(componentState.ServiceName)
            && await context.ServiceManager.ExistsAsync(componentState.ServiceName, cancellationToken)
        )
        {
            await context.ServiceManager.StopAsync(componentState.ServiceName, cancellationToken);
            await context.ServiceManager.DeleteAsync(componentState.ServiceName, cancellationToken);
        }

        if (
            purgeData
            && !string.IsNullOrWhiteSpace(componentState.InstalledPath)
            && Directory.Exists(componentState.InstalledPath)
        )
        {
            Directory.Delete(componentState.InstalledPath, recursive: true);
        }

        context.State.Components.Remove(componentId);
        return StepResult.Success($"Component '{componentId}' was removed.");
    }

    /// <inheritdoc />
    public Task RollbackAsync(StepContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
