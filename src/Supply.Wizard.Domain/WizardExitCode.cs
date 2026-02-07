namespace Supply.Wizard.Domain;

/// <summary>
/// Stable process exit codes for automation scenarios.
/// </summary>
public enum WizardExitCode
{
    /// <summary>
    /// Indicates success.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Indicates invalid input.
    /// </summary>
    InvalidInput = 10,

    /// <summary>
    /// Indicates api or authentication failure.
    /// </summary>
    ApiOrAuthenticationFailure = 20,

    /// <summary>
    /// Indicates manifest or artifact failure.
    /// </summary>
    ManifestOrArtifactFailure = 30,

    /// <summary>
    /// Indicates dependency validation failure.
    /// </summary>
    DependencyValidationFailure = 40,

    /// <summary>
    /// Indicates execution failure rollback succeeded.
    /// </summary>
    ExecutionFailureRollbackSucceeded = 50,

    /// <summary>
    /// Indicates execution failure rollback failed.
    /// </summary>
    ExecutionFailureRollbackFailed = 51,

    /// <summary>
    /// Indicates unexpected failure.
    /// </summary>
    UnexpectedFailure = 99,
}
