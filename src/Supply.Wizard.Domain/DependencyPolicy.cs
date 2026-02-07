namespace Supply.Wizard.Domain;

/// <summary>
/// Determines whether the wizard manages a dependency lifecycle.
/// </summary>
public enum DependencyPolicy
{
    /// <summary>
    /// Indicates managed.
    /// </summary>
    Managed,

    /// <summary>
    /// Indicates external.
    /// </summary>
    External,
}
