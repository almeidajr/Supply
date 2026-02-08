namespace Supply.Wizard.Application.Exceptions;

/// <summary>
/// Represents failed dependency checks.
/// </summary>
public sealed class DependencyValidationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
