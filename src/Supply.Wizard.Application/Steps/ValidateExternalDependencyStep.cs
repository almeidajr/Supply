using System.Net.Sockets;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Steps;

/// <summary>
/// Validates reachability for externally-managed dependencies.
/// </summary>
public sealed class ValidateExternalDependencyStep(string dependencyId, Uri endpoint) : IPlanStep
{
    /// <inheritdoc />
    public string Id => $"dependency.validate.{dependencyId}";

    /// <inheritdoc />
    public string Name => $"Validate external dependency '{dependencyId}'";

    /// <inheritdoc />
    public bool IsReversible => true;

    /// <inheritdoc />
    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        if (context.Request.DryRun)
        {
            return StepResult.Success($"Dry-run: skipped connectivity probe for {dependencyId}.");
        }

        var port = endpoint.IsDefaultPort
            ? endpoint.Scheme switch
            {
                "http" => 80,
                "https" => 443,
                _ => throw new DependencyValidationException(
                    $"Unsupported endpoint scheme '{endpoint.Scheme}' for dependency '{dependencyId}'."
                ),
            }
            : endpoint.Port;

        using var tcpClient = new TcpClient();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await tcpClient.ConnectAsync(endpoint.Host, port, timeoutSource.Token);
        }
        catch (Exception exception)
        {
            throw new DependencyValidationException(
                $"Failed to connect to external dependency '{dependencyId}' at '{endpoint}'.",
                exception
            );
        }

        context.State.ExternalDependencies[dependencyId] = new ExternalDependencyState
        {
            DependencyId = dependencyId,
            Endpoint = endpoint,
            ValidatedAtUtc = DateTimeOffset.UtcNow,
        };

        return StepResult.Success($"External dependency '{dependencyId}' is reachable.");
    }

    /// <inheritdoc />
    public Task RollbackAsync(StepContext context, CancellationToken cancellationToken)
    {
        context.State.ExternalDependencies.Remove(dependencyId);
        return Task.CompletedTask;
    }
}
