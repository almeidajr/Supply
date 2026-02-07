namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents dependency policy.
/// </summary>
public enum DependencyPolicy
{
    /// <summary>
    /// Indicates managed.
    /// </summary>
    Managed = 0,

    /// <summary>
    /// Indicates external.
    /// </summary>
    External = 1,
}
