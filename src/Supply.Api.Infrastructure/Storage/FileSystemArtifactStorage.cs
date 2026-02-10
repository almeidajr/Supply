using Microsoft.Extensions.Options;
using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Options;

namespace Supply.Api.Infrastructure.Storage;

/// <summary>
/// Provides artifact access backed by the local file system repository.
/// </summary>
/// <param name="options">Resolved API options that include the repository root path.</param>
public sealed class FileSystemArtifactStorage(IOptions<SupplyApiOptions> options) : IArtifactStorage
{
    private readonly SupplyApiOptions _supplyApiOptions = options.Value;

    /// <summary>
    /// Determines whether an artifact exists at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Path relative to the configured repository root.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the artifact exists; otherwise <see langword="false"/>.</returns>
    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = GetFullPath(relativePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    /// <summary>
    /// Gets file metadata for an artifact at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Path relative to the configured repository root.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>Artifact metadata when the file exists; otherwise <see langword="null"/>.</returns>
    public Task<ArtifactFileMetadata?> GetMetadataAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<ArtifactFileMetadata?>(null);
        }

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult<ArtifactFileMetadata?>(
            new ArtifactFileMetadata { SizeBytes = fileInfo.Length, LastWriteAtUtc = fileInfo.LastWriteTimeUtc }
        );
    }

    /// <summary>
    /// Opens a read-only stream for an artifact at the provided relative path.
    /// </summary>
    /// <param name="relativePath">Path relative to the configured repository root.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>A readable stream for the artifact content.</returns>
    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = GetFullPath(relativePath);
        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    private string GetFullPath(string relativePath)
    {
        var normalizedRelativePath = relativePath
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(_supplyApiOptions.RepositoryRootPath, normalizedRelativePath));
    }
}
