namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the catalog document.
/// </summary>
public sealed record CatalogDocument
{
    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Gets or sets the artifacts keyed by artifact identifier.
    /// </summary>
    public Dictionary<string, ArtifactDocument> Artifacts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the manifest releases keyed by release identifier.
    /// </summary>
    public Dictionary<string, ManifestReleaseDocument> ManifestReleases { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the wizard binary releases keyed by release identifier.
    /// </summary>
    public Dictionary<string, WizardBinaryReleaseDocument> WizardBinaryReleases { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the channel pointers keyed by channel name.
    /// </summary>
    public Dictionary<string, ChannelPointerDocument> ChannelPointers { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the customer policies keyed by customer identifier.
    /// </summary>
    public Dictionary<string, CustomerPolicyDocument> CustomerPolicies { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Represents the manifest release document.
/// </summary>
public sealed record ManifestReleaseDocument
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the release version.
    /// </summary>
    public required string ReleaseVersion { get; init; }

    /// <summary>
    /// Gets or sets the min wizard version.
    /// </summary>
    public required string MinWizardVersion { get; init; }

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    public IReadOnlyList<ManifestComponentDocument> Components { get; init; } = [];

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public IReadOnlyList<ManifestDependencyDocument> Dependencies { get; init; } = [];
}
