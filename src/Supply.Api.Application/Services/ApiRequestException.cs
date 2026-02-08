namespace Supply.Api.Application.Services;

public sealed class ApiRequestException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
