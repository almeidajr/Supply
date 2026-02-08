namespace Supply.Wizard.Application.Exceptions;

/// <summary>
/// Represents failures while retrieving data from Supply.Api.
/// </summary>
public sealed class ApiAccessException(string message, Exception? innerException = null)
    : Exception(message, innerException);
