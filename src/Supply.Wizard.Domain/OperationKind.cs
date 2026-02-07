namespace Supply.Wizard.Domain;

/// <summary>
/// Supported top-level operations for the wizard engine.
/// </summary>
public enum OperationKind
{
    /// <summary>
    /// Indicates install.
    /// </summary>
    Install,

    /// <summary>
    /// Indicates update.
    /// </summary>
    Update,

    /// <summary>
    /// Indicates uninstall.
    /// </summary>
    Uninstall,
}
