namespace Supply.Wizard.Domain;

/// <summary>
/// Service registration data used by platform-specific service managers.
/// </summary>
public sealed record ServiceDefinition
{
    public string ServiceName { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ExecutablePath { get; init; } = string.Empty;

    public IReadOnlyList<string> Arguments { get; init; } = [];

    public string WorkingDirectoryPath { get; init; } = string.Empty;

    public IDictionary<string, string> EnvironmentVariables { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
