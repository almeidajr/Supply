using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Retrieves install manifests from Supply.Api.
/// </summary>
public interface IManifestClient
{
    /// <summary>
    /// Retrieves a manifest document for the requested channel and endpoint.
    /// </summary>
    /// <param name="query">Manifest query options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The resolved manifest document.</returns>
    Task<ManifestDocument> GetManifestAsync(ManifestQuery query, CancellationToken cancellationToken);
}
