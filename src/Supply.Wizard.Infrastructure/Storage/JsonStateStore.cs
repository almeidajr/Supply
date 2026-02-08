using System.Text.Json;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Infrastructure.Storage;

/// <summary>
/// JSON-backed state and JSON-lines journal persistence.
/// </summary>
public sealed class JsonStateStore : IStateStore, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<WizardState> LoadAsync(string stateFilePath, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(stateFilePath))
            {
                return new WizardState();
            }

            await using var stream = File.OpenRead(stateFilePath);
            var state = await JsonSerializer.DeserializeAsync<WizardState>(
                stream,
                SerializerOptions,
                cancellationToken
            );

            return state ?? new WizardState();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(string stateFilePath, WizardState state, CancellationToken cancellationToken)
    {
        var directoryPath = Path.GetDirectoryName(stateFilePath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directoryPath);

        var temporaryFilePath = $"{stateFilePath}.tmp";

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using (var stream = File.Create(temporaryFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
            }

            File.Move(temporaryFilePath, stateFilePath, overwrite: true);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AppendJournalAsync(
        string journalFilePath,
        JournalEntry entry,
        CancellationToken cancellationToken
    )
    {
        var directoryPath = Path.GetDirectoryName(journalFilePath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directoryPath);

        var serialized = JsonSerializer.Serialize(entry, SerializerOptions);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(journalFilePath, serialized + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _gate.Dispose();
    }
}
