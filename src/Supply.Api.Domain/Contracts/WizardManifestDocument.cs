namespace Supply.Api.Domain.Contracts;

public sealed record WizardManifestDocument
{
    public string SchemaVersion { get; init; } = "1.0";

    public required string ManifestId { get; init; }

    public required string ManifestVersion { get; init; }

    public required DateTimeOffset PublishedAtUtc { get; init; }

    public required string Channel { get; init; }

    public required string CustomerId { get; init; }

    public required string ReleaseVersion { get; init; }

    public string MinWizardVersion { get; init; } = "1.0.0";

    public IReadOnlyList<ComponentManifestDocument> Components { get; init; } = [];

    public IReadOnlyList<DependencyManifestDocument> Dependencies { get; init; } = [];

    public Dictionary<string, bool> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
