using Supply.Api.Domain.Catalog;

namespace Supply.Api.Domain.Contracts;

public sealed record InternalUpsertReleaseRequest
{
    public ManifestReleaseDocument? ManifestRelease { get; init; }

    public WizardBinaryReleaseDocument? WizardBinaryRelease { get; init; }

    public IReadOnlyList<ArtifactDocument> Artifacts { get; init; } = [];
}
