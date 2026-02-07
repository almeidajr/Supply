namespace Supply.Api.Domain.Contracts;

public sealed record ComponentManifestDocument
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Version { get; init; }

    public bool EnabledByDefault { get; init; } = true;

    public IReadOnlyList<string> DependsOnComponentIds { get; init; } = [];

    public IReadOnlyList<string> DependencyIds { get; init; } = [];

    public IReadOnlyList<ArtifactManifestDocument> Artifacts { get; init; } = [];

    public required ServiceManifestDocument Service { get; init; }
}
