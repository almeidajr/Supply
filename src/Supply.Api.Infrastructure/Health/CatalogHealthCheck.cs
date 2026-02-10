using Microsoft.Extensions.Diagnostics.HealthChecks;
using Supply.Api.Application.Abstractions;

namespace Supply.Api.Infrastructure.Health;

/// <summary>
/// Validates that the release catalog can be read from the configured repository.
/// </summary>
/// <param name="releaseCatalogRepository">Catalog repository used to probe catalog availability.</param>
public sealed class CatalogHealthCheck(IReleaseCatalogRepository releaseCatalogRepository) : IHealthCheck
{
    /// <summary>
    /// Checks health by attempting to load the release catalog.
    /// </summary>
    /// <param name="context">Health check execution context.</param>
    /// <param name="cancellationToken">Cancellation token for the health check operation.</param>
    /// <returns>A healthy result when catalog load succeeds; otherwise an unhealthy result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await releaseCatalogRepository.GetCatalogAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Catalog is unavailable.", exception);
        }
    }
}
