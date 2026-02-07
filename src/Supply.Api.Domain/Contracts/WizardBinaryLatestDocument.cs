namespace Supply.Api.Domain.Contracts;

public sealed record WizardBinaryLatestDocument
{
    public required string Channel { get; init; }

    public required string Version { get; init; }

    public required string Os { get; init; }

    public required string Architecture { get; init; }

    public required long SizeBytes { get; init; }

    public required string Sha256 { get; init; }

    public required Uri DownloadUri { get; init; }

    public required string ETag { get; init; }

    public DateTimeOffset PublishedAtUtc { get; init; }
}
