using Supply.Wizard.Application;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Infrastructure.System.Services;

/// <summary>
/// Windows Service Control Manager adapter.
/// </summary>
public sealed class WindowsServiceManager(IProcessRunner processRunner) : IServiceManager
{
    /// <summary>
    /// Determines whether a Windows service exists.
    /// </summary>
    /// <param name="serviceName">Windows service name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the service exists; otherwise <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(string serviceName, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec { FileName = "sc.exe", Arguments = ["query", serviceName] },
            cancellationToken
        );

        return result.Succeeded;
    }

    /// <summary>
    /// Creates a new Windows service or updates an existing one.
    /// </summary>
    /// <param name="definition">Service definition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CreateOrUpdateAsync(ServiceDefinition definition, CancellationToken cancellationToken)
    {
        var binPath = BuildBinPath(definition);

        if (await ExistsAsync(definition.ServiceName, cancellationToken))
        {
            await RunCheckedAsync(
                [
                    "config",
                    definition.ServiceName,
                    $"binPath= {binPath}",
                    "start= auto",
                    $"DisplayName= {definition.DisplayName}",
                ],
                cancellationToken
            );

            return;
        }

        await RunCheckedAsync(
            [
                "create",
                definition.ServiceName,
                $"binPath= {binPath}",
                "start= auto",
                $"DisplayName= {definition.DisplayName}",
            ],
            cancellationToken
        );
    }

    /// <summary>
    /// Starts the specified Windows service.
    /// </summary>
    /// <param name="serviceName">Windows service name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task StartAsync(string serviceName, CancellationToken cancellationToken) =>
        RunCheckedAsync(["start", serviceName], cancellationToken);

    /// <summary>
    /// Stops the specified Windows service when it exists.
    /// </summary>
    /// <param name="serviceName">Windows service name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task StopAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync(serviceName, cancellationToken))
        {
            return;
        }

        await processRunner.RunAsync(
            new ProcessSpec { FileName = "sc.exe", Arguments = ["stop", serviceName] },
            cancellationToken
        );
    }

    /// <summary>
    /// Deletes the specified Windows service when it exists.
    /// </summary>
    /// <param name="serviceName">Windows service name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync(serviceName, cancellationToken))
        {
            return;
        }

        await processRunner.RunAsync(
            new ProcessSpec { FileName = "sc.exe", Arguments = ["delete", serviceName] },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the runtime status of the specified Windows service.
    /// </summary>
    /// <param name="serviceName">Windows service name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mapped service status.</returns>
    public async Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec { FileName = "sc.exe", Arguments = ["query", serviceName] },
            cancellationToken
        );

        if (!result.Succeeded)
        {
            return ServiceStatus.NotFound;
        }

        var output = result.StandardOutput;
        if (output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceStatus.Running;
        }

        if (output.Contains("STOPPED", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceStatus.Stopped;
        }

        return ServiceStatus.Unknown;
    }

    private static string BuildBinPath(ServiceDefinition definition)
    {
        var executablePath = Quote(definition.ExecutablePath);
        var arguments = string.Join(' ', definition.Arguments.Select(Quote));
        return string.IsNullOrWhiteSpace(arguments) ? executablePath : $"{executablePath} {arguments}";
    }

    private static string Quote(string value) => value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;

    private async Task RunCheckedAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            new ProcessSpec { FileName = "sc.exe", Arguments = arguments },
            cancellationToken
        );

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"sc.exe {string.Join(' ', arguments)} failed with code {result.ExitCode}: {result.StandardError}"
            );
        }
    }
}
