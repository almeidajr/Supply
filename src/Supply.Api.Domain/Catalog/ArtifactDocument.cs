namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the artifact document.
/// </summary>
public sealed record ArtifactDocument
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the relative path.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or sets the size bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets or sets the sha256.
    /// </summary>
    public required string Sha256 { get; init; }

    /// <summary>
    /// Gets or sets the package type.
    /// </summary>
    public required string PackageType { get; init; }

    /// <summary>
    /// Gets or sets the os.
    /// </summary>
    public required string Os { get; init; }

    /// <summary>
    /// Gets or sets the architecture.
    /// </summary>
    public required string Architecture { get; init; }

    /// <summary>
    /// Gets or sets the published at utc.
    /// </summary>
    public required DateTimeOffset PublishedAtUtc { get; init; }
}
