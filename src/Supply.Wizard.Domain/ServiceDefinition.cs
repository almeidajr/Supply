namespace Supply.Wizard.Domain;

/// <summary>
/// Service registration data used by platform-specific service managers.
/// </summary>
public sealed record ServiceDefinition
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the executable path.
    /// </summary>
    public string ExecutablePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Gets or sets the working directory path.
    /// </summary>
    public string WorkingDirectoryPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets environment variable values keyed by variable name.
    /// </summary>
    public IDictionary<string, string> EnvironmentVariables { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
