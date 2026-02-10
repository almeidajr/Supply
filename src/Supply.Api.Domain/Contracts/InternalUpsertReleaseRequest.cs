using System.ComponentModel;
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
    [Description("Manifest release payload to insert or update.")]
    public ManifestReleaseDocument? ManifestRelease { get; init; }

    /// <summary>
    /// Gets or sets the wizard binary release.
    /// </summary>
    [Description("Wizard binary release payload to insert or update.")]
    public WizardBinaryReleaseDocument? WizardBinaryRelease { get; init; }

    /// <summary>
    /// Gets or sets the artifacts.
    /// </summary>
    [Description("Artifact payloads to insert or update.")]
    public IReadOnlyList<ArtifactDocument> Artifacts { get; init; } = [];
}
