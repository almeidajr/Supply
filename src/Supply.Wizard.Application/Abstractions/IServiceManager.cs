using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Cross-platform abstraction for service lifecycle operations.
/// </summary>
public interface IServiceManager
{
    /// <summary>
    /// Checks whether a service exists on the host.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns><see langword="true"/> if the service exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string serviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a managed service definition.
    /// </summary>
    /// <param name="definition">Service definition to apply.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when the service definition is applied.</returns>
    Task CreateOrUpdateAsync(ServiceDefinition definition, CancellationToken cancellationToken);

    /// <summary>
    /// Starts a service.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when the service starts.</returns>
    Task StartAsync(string serviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Stops a service.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when the service stops.</returns>
    Task StopAsync(string serviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a service definition.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when the service is removed.</returns>
    Task DeleteAsync(string serviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current service status.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The current service status.</returns>
    Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken);
}
