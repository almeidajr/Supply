using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Creates executable plans for install, update, and uninstall operations.
/// </summary>
public interface IPlanBuilder
{
    /// <summary>
    /// Builds an execution plan from the request, manifest, and current state.
    /// </summary>
    /// <param name="request">Wizard request describing the target operation.</param>
    /// <param name="manifest">Manifest document used for planning.</param>
    /// <param name="state">Current persisted wizard state.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An execution plan for the requested operation.</returns>
    Task<ExecutionPlan> BuildAsync(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state,
        CancellationToken cancellationToken
    );
}
