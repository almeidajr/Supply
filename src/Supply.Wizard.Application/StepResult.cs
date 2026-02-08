namespace Supply.Wizard.Application;

/// <summary>
/// Individual step execution outcome.
/// </summary>
public sealed record StepResult
{
    /// <summary>
    /// Creates a successful step result.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful step result.</returns>
    public static StepResult Success(string message = "") => new() { Succeeded = true, Message = message };

    /// <summary>
    /// Creates a failed step result.
    /// </summary>
    /// <param name="message">Failure message.</param>
    /// <returns>A failed step result.</returns>
    public static StepResult Failure(string message) => new() { Succeeded = false, Message = message };

    /// <summary>
    /// Gets a value indicating whether the step succeeded.
    /// </summary>
    public required bool Succeeded { get; init; }

    /// <summary>
    /// Gets the step outcome message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
