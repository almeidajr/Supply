namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Provides artifact storage access for the distribution API.
/// </summary>
public interface IArtifactStorage
{
    /// <summary>
    /// Determines whether an artifact exists at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Artifact path relative to repository root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the artifact exists; otherwise <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Gets metadata for an artifact at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Artifact path relative to repository root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Artifact metadata when found; otherwise <see langword="null"/>.</returns>
    Task<ArtifactFileMetadata?> GetMetadataAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Opens a read-only stream for the artifact at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Artifact path relative to repository root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A readable stream for artifact content.</returns>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
}

/// <summary>
/// Describes artifact file metadata from storage.
/// </summary>
public sealed record ArtifactFileMetadata
{
    /// <summary>
    /// Gets the artifact size in bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets the artifact last-write timestamp in UTC.
    /// </summary>
    public DateTimeOffset LastWriteAtUtc { get; init; }
}
