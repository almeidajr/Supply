namespace Supply.Wizard.Domain;

/// <summary>
/// Transport-level security options for API connectivity.
/// </summary>
public sealed record WizardTlsOptions
{
    public bool AllowInsecureServerCertificate { get; init; }

    public string? CustomCaCertificateFilePath { get; init; }
}
