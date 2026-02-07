namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the wizard binary latest document.
/// </summary>
public sealed record WizardBinaryLatestDocument
{
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets or sets the os.
    /// </summary>
    public required string Os { get; init; }

    /// <summary>
    /// Gets or sets the architecture.
    /// </summary>
    public required string Architecture { get; init; }

    /// <summary>
    /// Gets or sets the size bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets or sets the sha256.
    /// </summary>
    public required string Sha256 { get; init; }

    /// <summary>
    /// Gets or sets the download uri.
    /// </summary>
    public required Uri DownloadUri { get; init; }

    /// <summary>
    /// Gets or sets the e tag.
    /// </summary>
    public required string ETag { get; init; }

    /// <summary>
    /// Gets or sets the published at utc.
    /// </summary>
    public DateTimeOffset PublishedAtUtc { get; init; }
}
