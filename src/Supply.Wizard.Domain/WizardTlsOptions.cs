namespace Supply.Wizard.Domain;

/// <summary>
/// Transport-level security options for API connectivity.
/// </summary>
public sealed record WizardTlsOptions
{
    /// <summary>
    /// Gets or sets the allow insecure server certificate.
    /// </summary>
    public bool AllowInsecureServerCertificate { get; init; }

    /// <summary>
    /// Gets or sets the custom ca certificate file path.
    /// </summary>
    public string? CustomCaCertificateFilePath { get; init; }
}
