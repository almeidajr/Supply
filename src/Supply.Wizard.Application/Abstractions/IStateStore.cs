using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Persists current install state and journal events.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Loads wizard state from persistent storage.
    /// </summary>
    /// <param name="stateFilePath">State file path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The current wizard state.</returns>
    Task<WizardState> LoadAsync(string stateFilePath, CancellationToken cancellationToken);

    /// <summary>
    /// Saves wizard state to persistent storage.
    /// </summary>
    /// <param name="stateFilePath">State file path.</param>
    /// <param name="state">State to persist.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when persistence finishes.</returns>
    Task SaveAsync(string stateFilePath, WizardState state, CancellationToken cancellationToken);

    /// <summary>
    /// Appends one journal entry to the run journal.
    /// </summary>
    /// <param name="journalFilePath">Journal file path.</param>
    /// <param name="entry">Journal entry to append.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when the entry is appended.</returns>
    Task AppendJournalAsync(string journalFilePath, JournalEntry entry, CancellationToken cancellationToken);
}
