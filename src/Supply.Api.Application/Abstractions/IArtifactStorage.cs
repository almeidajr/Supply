namespace Supply.Api.Application.Abstractions;

public interface IArtifactStorage
{
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken);

    Task<ArtifactFileMetadata?> GetMetadataAsync(string relativePath, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
}

public sealed record ArtifactFileMetadata
{
    public required long SizeBytes { get; init; }

    public DateTimeOffset LastWriteAtUtc { get; init; }
}
