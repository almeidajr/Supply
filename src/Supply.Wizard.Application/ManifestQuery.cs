using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Query metadata for fetching a manifest document.
/// </summary>
public sealed record ManifestQuery
{
    /// <summary>
    /// Gets the API base URI used for manifest retrieval.
    /// </summary>
    public required Uri ApiBaseUri { get; init; }

    /// <summary>
    /// Gets the release channel to query.
    /// </summary>
    public string Channel { get; init; } = "stable";

    /// <summary>
    /// Gets authentication options used for manifest retrieval.
    /// </summary>
    public WizardAuthOptions Authentication { get; init; } = new();

    /// <summary>
    /// Gets TLS options used for manifest retrieval.
    /// </summary>
    public WizardTlsOptions Tls { get; init; } = new();
}
