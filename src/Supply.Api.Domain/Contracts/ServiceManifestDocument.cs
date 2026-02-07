namespace Supply.Api.Domain.Contracts;

public sealed record ServiceManifestDocument
{
    public required string ServiceName { get; init; }

    public required string DisplayName { get; init; }

    public required string ExecutablePath { get; init; }

    public IReadOnlyList<string> Arguments { get; init; } = [];

    public required string WorkingDirectoryPath { get; init; }

    public Dictionary<string, string> EnvironmentVariables { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> DefaultInstallPaths { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
