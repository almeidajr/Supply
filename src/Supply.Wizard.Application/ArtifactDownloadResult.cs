namespace Supply.Wizard.Application;

/// <summary>
/// Artifact download output metadata.
/// </summary>
public sealed record ArtifactDownloadResult
{
    /// <summary>
    /// Gets the local file path for the downloaded artifact.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether an existing cached file was reused.
    /// </summary>
    public bool ReusedCachedFile { get; init; }
}
