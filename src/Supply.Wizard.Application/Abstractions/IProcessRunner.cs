namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Executes platform commands used by infrastructure adapters.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Executes an operating-system process.
    /// </summary>
    /// <param name="spec">Process specification.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The process result.</returns>
    Task<ProcessResult> RunAsync(ProcessSpec spec, CancellationToken cancellationToken);
}
