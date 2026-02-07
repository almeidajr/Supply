namespace Supply.Wizard.Domain;

/// <summary>
/// Stable process exit codes for automation scenarios.
/// </summary>
public enum WizardExitCode
{
    Success = 0,
    InvalidInput = 10,
    ApiOrAuthenticationFailure = 20,
    ManifestOrArtifactFailure = 30,
    DependencyValidationFailure = 40,
    ExecutionFailureRollbackSucceeded = 50,
    ExecutionFailureRollbackFailed = 51,
    UnexpectedFailure = 99,
}
