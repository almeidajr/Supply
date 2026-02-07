namespace Supply.Wizard.Domain;

/// <summary>
/// Append-only event stored for run diagnostics and recovery.
/// </summary>
public sealed record JournalEntry
{
    public Guid RunId { get; init; }

    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    public string EventType { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? StepId { get; init; }

    public string? ComponentId { get; init; }
}
