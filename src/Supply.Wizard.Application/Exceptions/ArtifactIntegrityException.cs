namespace Supply.Wizard.Application.Exceptions;

/// <summary>
/// Represents an artifact download or checksum validation failure.
/// </summary>
public sealed class ArtifactIntegrityException(string message, Exception? innerException = null)
    : Exception(message, innerException);
