using Supply.Wizard.Application;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Infrastructure.System.Services;

/// <summary>
/// systemd-backed service manager for Linux.
/// </summary>
public sealed class SystemdServiceManager(IProcessRunner processRunner) : IServiceManager
{
    private static readonly string DefaultUnitDirectoryPath = Path.Combine(
        Path.DirectorySeparatorChar.ToString(),
        "etc",
        "systemd",
        "system"
    );

    private readonly string _unitDirectoryPath = DefaultUnitDirectoryPath;

    /// <summary>
    /// Creates a systemd manager with an explicit unit file directory.
    /// </summary>
    /// <param name="processRunner">Process runner used to invoke systemctl.</param>
    /// <param name="unitDirectoryPath">Directory path where unit files are written.</param>
    public SystemdServiceManager(IProcessRunner processRunner, string unitDirectoryPath)
        : this(processRunner)
    {
        _unitDirectoryPath = string.IsNullOrWhiteSpace(unitDirectoryPath)
            ? DefaultUnitDirectoryPath
            : unitDirectoryPath;
    }

    /// <summary>
    /// Determines whether the service exists in systemd.
    /// </summary>
    /// <param name="serviceName">Service unit name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the service is present; otherwise <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(string serviceName, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec
            {
                FileName = "systemctl",
                Arguments = ["show", serviceName, "--property=LoadState", "--value"],
            },
            cancellationToken
        );

        return result.Succeeded
            && !string.Equals(result.StandardOutput.Trim(), "not-found", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates or updates the systemd unit file and enables the service.
    /// </summary>
    /// <param name="definition">Service definition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CreateOrUpdateAsync(ServiceDefinition definition, CancellationToken cancellationToken)
    {
        var unitFilePath = GetUnitFilePath(definition.ServiceName);
        var unitBody = BuildUnitBody(definition);

        Directory.CreateDirectory(Path.GetDirectoryName(unitFilePath) ?? DefaultUnitDirectoryPath);
        await File.WriteAllTextAsync(unitFilePath, unitBody, cancellationToken);

        await RunCheckedAsync("systemctl", ["daemon-reload"], cancellationToken);
        await RunCheckedAsync("systemctl", ["enable", definition.ServiceName], cancellationToken);
    }

    /// <summary>
    /// Starts the service via systemd.
    /// </summary>
    /// <param name="serviceName">Service unit name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task StartAsync(string serviceName, CancellationToken cancellationToken) =>
        RunCheckedAsync("systemctl", ["start", serviceName], cancellationToken);

    /// <summary>
    /// Stops the service when it exists.
    /// </summary>
    /// <param name="serviceName">Service unit name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task StopAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync(serviceName, cancellationToken))
        {
            return;
        }

        await RunCheckedAsync("systemctl", ["stop", serviceName], cancellationToken);
    }

    /// <summary>
    /// Disables and removes a service unit when it exists.
    /// </summary>
    /// <param name="serviceName">Service unit name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync(serviceName, cancellationToken))
        {
            return;
        }

        await processRunner.RunAsync(
            new ProcessSpec { FileName = "systemctl", Arguments = ["disable", serviceName] },
            cancellationToken
        );

        var unitFilePath = GetUnitFilePath(serviceName);
        if (File.Exists(unitFilePath))
        {
            File.Delete(unitFilePath);
        }

        await RunCheckedAsync("systemctl", ["daemon-reload"], cancellationToken);
    }

    /// <summary>
    /// Resolves current runtime status for the specified service.
    /// </summary>
    /// <param name="serviceName">Service unit name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mapped service status.</returns>
    public async Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec { FileName = "systemctl", Arguments = ["is-active", serviceName] },
            cancellationToken
        );

        if (!result.Succeeded)
        {
            return await ExistsAsync(serviceName, cancellationToken) ? ServiceStatus.Stopped : ServiceStatus.NotFound;
        }

        return string.Equals(result.StandardOutput.Trim(), "active", StringComparison.OrdinalIgnoreCase)
            ? ServiceStatus.Running
            : ServiceStatus.Stopped;
    }

    private static string BuildUnitBody(ServiceDefinition definition)
    {
        var arguments = string.Join(' ', definition.Arguments.Select(EscapeArgument));
        var execStart = string.IsNullOrWhiteSpace(arguments)
            ? EscapeArgument(definition.ExecutablePath)
            : $"{EscapeArgument(definition.ExecutablePath)} {arguments}";

        var environmentSection = string.Join(
            Environment.NewLine,
            definition.EnvironmentVariables.Select(pair =>
                $"Environment=\"{pair.Key}={EscapeEnvironmentValue(pair.Value)}\""
            )
        );

        var workingDirectory = string.IsNullOrWhiteSpace(definition.WorkingDirectoryPath)
            ? Path.GetDirectoryName(definition.ExecutablePath) ?? "/"
            : definition.WorkingDirectoryPath;

        return $$"""
            [Unit]
            Description={{definition.DisplayName}}
            After=network.target

            [Service]
            Type=simple
            WorkingDirectory={{workingDirectory}}
            ExecStart={{execStart}}
            Restart=always
            {{environmentSection}}

            [Install]
            WantedBy=multi-user.target
            """;
    }

    private static string EscapeArgument(string value) =>
        value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;

    private static string EscapeEnvironmentValue(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private string GetUnitFilePath(string serviceName) => Path.Combine(_unitDirectoryPath, $"{serviceName}.service");

    private async Task RunCheckedAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken
    )
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec { FileName = fileName, Arguments = arguments },
            cancellationToken
        );

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{fileName} {string.Join(' ', arguments)} failed with code {result.ExitCode}: {result.StandardError}"
            );
        }
    }
}
