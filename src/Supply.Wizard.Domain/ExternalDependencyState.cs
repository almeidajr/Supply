namespace Supply.Wizard.Domain;

/// <summary>
/// Persisted endpoint for dependencies managed externally.
/// </summary>
public sealed record ExternalDependencyState
{
    public string DependencyId { get; init; } = string.Empty;

    public Uri Endpoint { get; init; } = new("https://localhost");

    public DateTimeOffset ValidatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
