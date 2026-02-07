namespace Supply.Api.Domain.Options;

public sealed class SupplyApiOptions
{
    public const string ConfigurationSectionName = "SupplyApi";

    public string RepositoryRootPath { get; init; } =
        Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "repository");

    public string CatalogFilePath { get; init; } =
        Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "repository", "catalog.json");

    public string DefaultChannel { get; init; } = "stable";

    public bool RequireAuthentication { get; init; }

    public string? InternalApiKey { get; init; }

    public Dictionary<string, CustomerPolicyOptions> Customers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
