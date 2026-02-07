namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents a request to manifest.
/// </summary>
public sealed record ManifestRequest
{
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets or sets the operating system.
    /// </summary>
    public string? OperatingSystem { get; init; }

    /// <summary>
    /// Gets or sets the architecture.
    /// </summary>
    public string? Architecture { get; init; }

    /// <summary>
    /// Gets or sets the wizard version.
    /// </summary>
    public string? WizardVersion { get; init; }

    /// <summary>
    /// Gets or sets the base uri.
    /// </summary>
    public string? BaseUri { get; init; }
}
