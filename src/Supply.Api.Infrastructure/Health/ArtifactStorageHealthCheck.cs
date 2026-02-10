using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Supply.Api.Domain.Options;

namespace Supply.Api.Infrastructure.Health;

/// <summary>
/// Validates that the configured artifact repository path is accessible.
/// </summary>
/// <param name="options">Resolved API options that include repository storage paths.</param>
public sealed class ArtifactStorageHealthCheck(IOptions<SupplyApiOptions> options) : IHealthCheck
{
    private readonly SupplyApiOptions _supplyApiOptions = options.Value;

    /// <summary>
    /// Checks health by verifying the artifact repository root directory exists.
    /// </summary>
    /// <param name="context">Health check execution context.</param>
    /// <param name="cancellationToken">Cancellation token for the health check operation.</param>
    /// <returns>A healthy result when the repository path exists; otherwise a degraded result.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (!Directory.Exists(_supplyApiOptions.RepositoryRootPath))
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Artifact repository root '{_supplyApiOptions.RepositoryRootPath}' does not exist."
                )
            );
        }

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
