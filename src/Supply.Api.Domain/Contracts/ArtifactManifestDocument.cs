namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the artifact manifest document.
/// </summary>
public sealed record ArtifactManifestDocument
{
    /// <summary>
    /// Gets or sets the artifact id.
    /// </summary>
    public required string ArtifactId { get; init; }

    /// <summary>
    /// Gets or sets the os.
    /// </summary>
    public required string Os { get; init; }

    /// <summary>
    /// Gets or sets the architecture.
    /// </summary>
    public required string Architecture { get; init; }

    /// <summary>
    /// Gets or sets the package type.
    /// </summary>
    public required string PackageType { get; init; }

    /// <summary>
    /// Gets or sets the download uri.
    /// </summary>
    public required Uri DownloadUri { get; init; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or sets the size bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets or sets the sha256.
    /// </summary>
    public required string Sha256 { get; init; }

    /// <summary>
    /// Gets or sets the e tag.
    /// </summary>
    public required string ETag { get; init; }
}
