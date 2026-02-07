namespace Supply.Api.Domain.Contracts;

public sealed record ArtifactDownloadResult : IAsyncDisposable
{
    public required Stream ContentStream { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }

    public required long SizeBytes { get; init; }

    public required string ETag { get; init; }

    public DateTimeOffset? LastModifiedUtc { get; init; }

    public ValueTask DisposeAsync()
    {
        return ContentStream.DisposeAsync();
    }
}
