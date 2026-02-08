namespace Supply.Wizard.Application;

/// <summary>
/// Process execution output.
/// </summary>
public sealed record ProcessResult
{
    /// <summary>
    /// Gets the process exit code.
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Gets captured standard output text.
    /// </summary>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>
    /// Gets captured standard error text.
    /// </summary>
    public string StandardError { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the process exited with code 0.
    /// </summary>
    public bool Succeeded => ExitCode is 0;
}
