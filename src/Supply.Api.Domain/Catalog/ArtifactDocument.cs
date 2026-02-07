namespace Supply.Api.Domain.Catalog;

public sealed record ArtifactDocument
{
    public required string Id { get; init; }

    public required string RelativePath { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }

    public required string Sha256 { get; init; }

    public required string PackageType { get; init; }

    public required string Os { get; init; }

    public required string Architecture { get; init; }

    public required DateTimeOffset PublishedAtUtc { get; init; }
}
