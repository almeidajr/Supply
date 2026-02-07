namespace Supply.Api.Domain.Options;

/// <summary>
/// Represents configuration options for supply api options.
/// </summary>
public sealed class SupplyApiOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string ConfigurationSectionName = "SupplyApi";

    /// <summary>
    /// Gets or sets the repository root path.
    /// </summary>
    public string RepositoryRootPath { get; init; } =
        Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "repository");

    /// <summary>
    /// Gets or sets the catalog file path.
    /// </summary>
    public string CatalogFilePath { get; init; } =
        Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "repository", "catalog.json");

    /// <summary>
    /// Gets or sets the default channel.
    /// </summary>
    public string DefaultChannel { get; init; } = "stable";

    /// <summary>
    /// Gets or sets the require authentication.
    /// </summary>
    public bool RequireAuthentication { get; init; }

    /// <summary>
    /// Gets or sets the internal api key.
    /// </summary>
    public string? InternalApiKey { get; init; }

    /// <summary>
    /// Gets or sets customer-specific policy options keyed by customer identifier.
    /// </summary>
    public Dictionary<string, CustomerPolicyOptions> Customers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
