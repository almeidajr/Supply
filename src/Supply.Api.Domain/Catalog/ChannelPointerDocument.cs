namespace Supply.Api.Domain.Catalog;

public sealed record ChannelPointerDocument
{
    public required string Channel { get; init; }

    public required string ManifestReleaseId { get; init; }

    public required string WizardBinaryReleaseId { get; init; }
}
