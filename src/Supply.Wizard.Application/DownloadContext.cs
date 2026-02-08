using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Context for artifact download destination.
/// </summary>
public sealed record DownloadContext
{
    /// <summary>
    /// Gets the directory path used for downloaded artifact caching.
    /// </summary>
    public required string CacheDirectoryPath { get; init; }

    /// <summary>
    /// Gets authentication options used for artifact requests.
    /// </summary>
    public WizardAuthOptions Authentication { get; init; } = new();

    /// <summary>
    /// Gets TLS options used for artifact requests.
    /// </summary>
    public WizardTlsOptions Tls { get; init; } = new();
}
