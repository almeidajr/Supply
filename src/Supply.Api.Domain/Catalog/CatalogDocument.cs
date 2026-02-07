namespace Supply.Api.Domain.Catalog;

public sealed record CatalogDocument
{
    public int SchemaVersion { get; init; } = 1;

    public Dictionary<string, ArtifactDocument> Artifacts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ManifestReleaseDocument> ManifestReleases { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, WizardBinaryReleaseDocument> WizardBinaryReleases { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ChannelPointerDocument> ChannelPointers { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, CustomerPolicyDocument> CustomerPolicies { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record ManifestReleaseDocument
{
    public required string Id { get; init; }

    public required string Channel { get; init; }

    public required string ReleaseVersion { get; init; }

    public required string MinWizardVersion { get; init; }

    public IReadOnlyList<ManifestComponentDocument> Components { get; init; } = [];

    public IReadOnlyList<ManifestDependencyDocument> Dependencies { get; init; } = [];
}
