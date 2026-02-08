namespace Supply.Api.Application.Services;

/// <summary>
/// Represents a request-level API error with an associated HTTP status code.
/// </summary>
/// <param name="message">Error message.</param>
/// <param name="statusCode">HTTP status code to return.</param>
public sealed class ApiRequestException(string message, int statusCode) : Exception(message)
{
    /// <summary>
    /// Gets the HTTP status code for this error.
    /// </summary>
    public int StatusCode { get; } = statusCode;
}
