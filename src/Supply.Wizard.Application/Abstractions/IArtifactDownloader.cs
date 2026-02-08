using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Downloads artifacts referenced by manifest entries.
/// </summary>
public interface IArtifactDownloader
{
    /// <summary>
    /// Downloads the artifact to a local path and returns download metadata.
    /// </summary>
    /// <param name="artifact">Artifact metadata from the manifest.</param>
    /// <param name="context">Download context including cache and transport settings.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The local artifact path and cache reuse information.</returns>
    Task<ArtifactDownloadResult> DownloadAsync(
        ArtifactManifest artifact,
        DownloadContext context,
        CancellationToken cancellationToken
    );
}
