namespace Supply.Api.Domain.Contracts;

public sealed record InternalPublishChannelRequest
{
    public required string Channel { get; init; }

    public string? ManifestReleaseId { get; init; }

    public string? WizardBinaryReleaseId { get; init; }
}
