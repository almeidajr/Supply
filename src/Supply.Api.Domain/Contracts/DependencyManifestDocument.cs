namespace Supply.Api.Domain.Contracts;

public sealed record DependencyManifestDocument
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public DependencyPolicy DefaultPolicy { get; init; } = DependencyPolicy.Managed;

    public string? ManagedComponentId { get; init; }

    public string? ProbeScheme { get; init; }

    public int? ProbePort { get; init; }
}
