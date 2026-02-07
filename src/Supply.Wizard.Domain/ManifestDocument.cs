namespace Supply.Wizard.Domain;

/// <summary>
/// Manifest payload returned by Supply.Api for edge deployment planning.
/// </summary>
public sealed record ManifestDocument
{
    public string ManifestVersion { get; init; } = string.Empty;

    public DateTimeOffset PublishedAtUtc { get; init; }

    public IReadOnlyList<ComponentManifest> Components { get; init; } = [];

    public IReadOnlyList<DependencyManifest> Dependencies { get; init; } = [];
}
