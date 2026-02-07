namespace Supply.Wizard.Domain;

/// <summary>
/// Supported top-level operations for the wizard engine.
/// </summary>
public enum OperationKind
{
    Install,
    Update,
    Uninstall,
}
