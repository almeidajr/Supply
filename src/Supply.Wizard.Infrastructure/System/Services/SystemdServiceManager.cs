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

    public SystemdServiceManager(IProcessRunner processRunner, string unitDirectoryPath)
        : this(processRunner)
    {
        _unitDirectoryPath = string.IsNullOrWhiteSpace(unitDirectoryPath)
            ? DefaultUnitDirectoryPath
            : unitDirectoryPath;
    }

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

    public async Task CreateOrUpdateAsync(ServiceDefinition definition, CancellationToken cancellationToken)
    {
        var unitFilePath = GetUnitFilePath(definition.ServiceName);
        var unitBody = BuildUnitBody(definition);

        Directory.CreateDirectory(Path.GetDirectoryName(unitFilePath) ?? DefaultUnitDirectoryPath);
        await File.WriteAllTextAsync(unitFilePath, unitBody, cancellationToken);

        await RunCheckedAsync("systemctl", ["daemon-reload"], cancellationToken);
        await RunCheckedAsync("systemctl", ["enable", definition.ServiceName], cancellationToken);
    }

    public Task StartAsync(string serviceName, CancellationToken cancellationToken)
    {
        return RunCheckedAsync("systemctl", ["start", serviceName], cancellationToken);
    }

    public async Task StopAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync(serviceName, cancellationToken))
        {
            return;
        }

        await RunCheckedAsync("systemctl", ["stop", serviceName], cancellationToken);
    }

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

    private static string EscapeArgument(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
    }

    private static string EscapeEnvironmentValue(string value)
    {
        return value.Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private string GetUnitFilePath(string serviceName)
    {
        return Path.Combine(_unitDirectoryPath, $"{serviceName}.service");
    }

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
