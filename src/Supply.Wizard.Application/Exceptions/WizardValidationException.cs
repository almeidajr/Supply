namespace Supply.Wizard.Application.Exceptions;

/// <summary>
/// Represents a validation error that should map to deterministic exit codes.
/// </summary>
public sealed class WizardValidationException(string message) : Exception(message);
