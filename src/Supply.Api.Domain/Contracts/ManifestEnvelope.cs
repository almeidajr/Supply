namespace Supply.Api.Domain.Contracts;

public sealed record ManifestEnvelope
{
    public required WizardManifestDocument Document { get; init; }

    public required string ETag { get; init; }
}
