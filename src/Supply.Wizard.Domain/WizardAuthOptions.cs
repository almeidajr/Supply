namespace Supply.Wizard.Domain;

/// <summary>
/// Authentication options used for API and artifact downloads.
/// </summary>
public sealed record WizardAuthOptions
{
    public string? BearerToken { get; init; }

    public string? BearerTokenFilePath { get; init; }

    public string? ClientCertificateFilePath { get; init; }

    public string? ClientCertificateKeyFilePath { get; init; }
}
