using Supply.Api.Domain.Contracts;

namespace Supply.Api.Domain.Catalog;

public sealed record ManifestDependencyDocument
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public DependencyPolicy DefaultPolicy { get; init; }

    public string? ManagedComponentId { get; init; }

    public string? ProbeScheme { get; init; }

    public int? ProbePort { get; init; }
}
