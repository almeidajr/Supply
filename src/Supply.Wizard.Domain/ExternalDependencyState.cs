namespace Supply.Wizard.Domain;

/// <summary>
/// Persisted endpoint for dependencies managed externally.
/// </summary>
public sealed record ExternalDependencyState
{
    /// <summary>
    /// Gets or sets the dependency id.
    /// </summary>
    public string DependencyId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the endpoint.
    /// </summary>
    public Uri Endpoint { get; init; } = new("https://localhost");

    /// <summary>
    /// Gets or sets the validated at utc.
    /// </summary>
    public DateTimeOffset ValidatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
