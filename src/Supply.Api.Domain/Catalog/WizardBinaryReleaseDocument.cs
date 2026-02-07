namespace Supply.Api.Domain.Catalog;

public sealed record WizardBinaryReleaseDocument
{
    public required string Id { get; init; }

    public required string Channel { get; init; }

    public required string Version { get; init; }

    public IReadOnlyList<string> ArtifactIds { get; init; } = [];
}
