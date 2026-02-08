using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Resolves wizard binary and artifact distribution resources.
/// </summary>
public interface IWizardDistributionService
{
    /// <summary>
    /// Gets the latest wizard binary metadata for a channel and platform.
    /// </summary>
    /// <param name="channel">Release channel.</param>
    /// <param name="operatingSystem">Target operating system.</param>
    /// <param name="architecture">Target architecture.</param>
    /// <param name="baseUri">Optional absolute base URI used to build download links.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Latest wizard binary metadata, or <see langword="null"/> when unavailable.</returns>
    Task<WizardBinaryLatestDocument?> GetLatestWizardBinaryAsync(
        string channel,
        string operatingSystem,
        string architecture,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Opens a wizard binary stream for a specific version and platform.
    /// </summary>
    /// <param name="version">Wizard version.</param>
    /// <param name="operatingSystem">Target operating system.</param>
    /// <param name="architecture">Target architecture.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Download stream metadata, or <see langword="null"/> when unavailable.</returns>
    Task<ArtifactDownloadResult?> OpenWizardBinaryAsync(
        string version,
        string operatingSystem,
        string architecture,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Opens a generic artifact stream by artifact identifier.
    /// </summary>
    /// <param name="artifactId">Artifact identifier.</param>
    /// <param name="customerContext">Resolved customer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Download stream metadata, or <see langword="null"/> when unavailable.</returns>
    Task<ArtifactDownloadResult?> OpenArtifactAsync(
        string artifactId,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
