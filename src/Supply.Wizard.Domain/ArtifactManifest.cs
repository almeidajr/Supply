namespace Supply.Wizard.Domain;

/// <summary>
/// Artifact variant for a component and target platform.
/// </summary>
public sealed record ArtifactManifest
{
    public string Os { get; init; } = string.Empty;

    public string Architecture { get; init; } = string.Empty;

    public Uri DownloadUri { get; init; } = new("https://localhost");

    public string FileName { get; init; } = string.Empty;

    public string Sha256 { get; init; } = string.Empty;
}
