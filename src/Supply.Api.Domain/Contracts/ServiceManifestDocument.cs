namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the service manifest document.
/// </summary>
public sealed record ServiceManifestDocument
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the executable path.
    /// </summary>
    public required string ExecutablePath { get; init; }

    /// <summary>
    /// Gets or sets the arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Gets or sets the working directory path.
    /// </summary>
    public required string WorkingDirectoryPath { get; init; }

    /// <summary>
    /// Gets or sets environment variable values by variable name.
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets default install paths by operating system key.
    /// </summary>
    public Dictionary<string, string> DefaultInstallPaths { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
