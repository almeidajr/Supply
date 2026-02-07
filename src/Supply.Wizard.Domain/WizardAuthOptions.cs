namespace Supply.Wizard.Domain;

/// <summary>
/// Authentication options used for API and artifact downloads.
/// </summary>
public sealed record WizardAuthOptions
{
    /// <summary>
    /// Gets or sets the bearer token.
    /// </summary>
    public string? BearerToken { get; init; }

    /// <summary>
    /// Gets or sets the bearer token file path.
    /// </summary>
    public string? BearerTokenFilePath { get; init; }

    /// <summary>
    /// Gets or sets the client certificate file path.
    /// </summary>
    public string? ClientCertificateFilePath { get; init; }

    /// <summary>
    /// Gets or sets the client certificate key file path.
    /// </summary>
    public string? ClientCertificateKeyFilePath { get; init; }
}
