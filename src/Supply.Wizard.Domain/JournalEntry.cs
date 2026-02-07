namespace Supply.Wizard.Domain;

/// <summary>
/// Append-only event stored for run diagnostics and recovery.
/// </summary>
public sealed record JournalEntry
{
    /// <summary>
    /// Gets or sets the run id.
    /// </summary>
    public Guid RunId { get; init; }

    /// <summary>
    /// Gets or sets the timestamp utc.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the step id.
    /// </summary>
    public string? StepId { get; init; }

    /// <summary>
    /// Gets or sets the component id.
    /// </summary>
    public string? ComponentId { get; init; }
}
