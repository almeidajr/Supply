namespace Supply.Wizard.Application;

/// <summary>
/// Process invocation details.
/// </summary>
public sealed record ProcessSpec
{
    /// <summary>
    /// Gets the executable file name or path.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the process arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Gets the working directory path.
    /// </summary>
    public string? WorkingDirectoryPath { get; init; }

    /// <summary>
    /// Gets environment variables to apply for process execution.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
