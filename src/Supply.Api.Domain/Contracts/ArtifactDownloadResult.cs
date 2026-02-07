namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the result of artifact download.
/// </summary>
public sealed record ArtifactDownloadResult : IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the content stream.
    /// </summary>
    public required Stream ContentStream { get; init; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or sets the size bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets or sets the e tag.
    /// </summary>
    public required string ETag { get; init; }

    /// <summary>
    /// Gets or sets the last modified utc.
    /// </summary>
    public DateTimeOffset? LastModifiedUtc { get; init; }

    /// <summary>
    /// Asynchronously disposes the underlying content stream.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ContentStream.DisposeAsync();
    }
}
