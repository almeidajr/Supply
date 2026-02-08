using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// High-level orchestrator for wizard operations.
/// </summary>
public interface IWizardEngine
{
    /// <summary>
    /// Executes a wizard request end-to-end.
    /// </summary>
    /// <param name="request">Requested operation and options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The wizard execution result.</returns>
    Task<WizardExecutionResult> ExecuteAsync(WizardRequest request, CancellationToken cancellationToken);
}
