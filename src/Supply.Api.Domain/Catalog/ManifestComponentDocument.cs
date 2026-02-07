namespace Supply.Api.Domain.Catalog;

public sealed record ManifestComponentDocument
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Version { get; init; }

    public bool EnabledByDefault { get; init; } = true;

    public IReadOnlyList<string> DependsOnComponentIds { get; init; } = [];

    public IReadOnlyList<string> DependencyIds { get; init; } = [];

    public IReadOnlyList<string> ArtifactIds { get; init; } = [];

    public required ServiceDocument Service { get; init; }
}
