namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents manifest envelope.
/// </summary>
public sealed record ManifestEnvelope
{
    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    public required WizardManifestDocument Document { get; init; }

    /// <summary>
    /// Gets or sets the e tag.
    /// </summary>
    public required string ETag { get; init; }
}
