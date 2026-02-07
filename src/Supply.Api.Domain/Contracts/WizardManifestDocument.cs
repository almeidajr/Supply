namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the wizard manifest document.
/// </summary>
public sealed record WizardManifestDocument
{
    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public string SchemaVersion { get; init; } = "1.0";

    /// <summary>
    /// Gets or sets the manifest id.
    /// </summary>
    public required string ManifestId { get; init; }

    /// <summary>
    /// Gets or sets the manifest version.
    /// </summary>
    public required string ManifestVersion { get; init; }

    /// <summary>
    /// Gets or sets the published at utc.
    /// </summary>
    public required DateTimeOffset PublishedAtUtc { get; init; }

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    public required string CustomerId { get; init; }

    /// <summary>
    /// Gets or sets the release version.
    /// </summary>
    public required string ReleaseVersion { get; init; }

    /// <summary>
    /// Gets or sets the min wizard version.
    /// </summary>
    public string MinWizardVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    public IReadOnlyList<ComponentManifestDocument> Components { get; init; } = [];

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public IReadOnlyList<DependencyManifestDocument> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets or sets feature flags keyed by feature name.
    /// </summary>
    public Dictionary<string, bool> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
