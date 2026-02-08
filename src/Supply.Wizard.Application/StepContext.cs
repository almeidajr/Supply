using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Shared mutable context used while running plan steps.
/// </summary>
public sealed class StepContext
{
    /// <summary>
    /// Gets the current run identifier.
    /// </summary>
    public required Guid RunId { get; init; }

    /// <summary>
    /// Gets the wizard request being executed.
    /// </summary>
    public required WizardRequest Request { get; init; }

    /// <summary>
    /// Gets the manifest used for the run.
    /// </summary>
    public required ManifestDocument Manifest { get; init; }

    /// <summary>
    /// Gets the artifact downloader service.
    /// </summary>
    public required IArtifactDownloader ArtifactDownloader { get; init; }

    /// <summary>
    /// Gets the checksum verifier service.
    /// </summary>
    public required IChecksumVerifier ChecksumVerifier { get; init; }

    /// <summary>
    /// Gets the state store service.
    /// </summary>
    public required IStateStore StateStore { get; init; }

    /// <summary>
    /// Gets the service manager abstraction.
    /// </summary>
    public required IServiceManager ServiceManager { get; init; }

    /// <summary>
    /// Gets the process runner abstraction.
    /// </summary>
    public required IProcessRunner ProcessRunner { get; init; }

    /// <summary>
    /// Gets mutable state for the current run.
    /// </summary>
    public required WizardState State { get; init; }
}
