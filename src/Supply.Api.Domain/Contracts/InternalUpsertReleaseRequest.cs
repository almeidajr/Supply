using Supply.Api.Domain.Catalog;

namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to internal upsert release.
/// </summary>
public sealed record InternalUpsertReleaseRequest
{
    /// <summary>
    /// Gets or sets the manifest release.
    /// </summary>
    public ManifestReleaseDocument? ManifestRelease { get; init; }

    /// <summary>
    /// Gets or sets the wizard binary release.
    /// </summary>
    public WizardBinaryReleaseDocument? WizardBinaryRelease { get; init; }

    /// <summary>
    /// Gets or sets the artifacts.
    /// </summary>
    public IReadOnlyList<ArtifactDocument> Artifacts { get; init; } = [];
}
