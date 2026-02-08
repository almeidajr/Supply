using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Produces manifest payloads for wizard clients.
/// </summary>
public interface IWizardManifestService
{
    /// <summary>
    /// Gets a manifest for the requested channel and customer context.
    /// </summary>
    /// <param name="request">Manifest request payload.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Manifest envelope, or <see langword="null"/> when no manifest is available.</returns>
    Task<ManifestEnvelope?> GetManifestAsync(
        ManifestRequest request,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
