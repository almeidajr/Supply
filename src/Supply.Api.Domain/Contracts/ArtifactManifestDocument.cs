namespace Supply.Api.Domain.Contracts;

public sealed record ArtifactManifestDocument
{
    public required string ArtifactId { get; init; }

    public required string Os { get; init; }

    public required string Architecture { get; init; }

    public required string PackageType { get; init; }

    public required Uri DownloadUri { get; init; }

    public required string FileName { get; init; }

    public required long SizeBytes { get; init; }

    public required string Sha256 { get; init; }

    public required string ETag { get; init; }
}
