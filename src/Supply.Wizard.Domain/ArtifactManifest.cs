namespace Supply.Wizard.Domain;

/// <summary>
/// Artifact variant for a component and target platform.
/// </summary>
public sealed record ArtifactManifest
{
    /// <summary>
    /// Gets or sets the os.
    /// </summary>
    public string Os { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the architecture.
    /// </summary>
    public string Architecture { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the download uri.
    /// </summary>
    public Uri DownloadUri { get; init; } = new("https://localhost");

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the sha256.
    /// </summary>
    public string Sha256 { get; init; } = string.Empty;
}
