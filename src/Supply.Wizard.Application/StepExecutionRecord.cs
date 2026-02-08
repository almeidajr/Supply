namespace Supply.Wizard.Application;

/// <summary>
/// Runtime status for each step.
/// </summary>
public sealed record StepExecutionRecord
{
    /// <summary>
    /// Gets the executed step identifier.
    /// </summary>
    public required string StepId { get; init; }

    /// <summary>
    /// Gets the executed step display name.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the step succeeded.
    /// </summary>
    public required bool Succeeded { get; init; }

    /// <summary>
    /// Gets the step outcome message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
