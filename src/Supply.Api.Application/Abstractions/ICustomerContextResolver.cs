using Microsoft.AspNetCore.Http;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

/// <summary>
/// Resolves customer context for the current API request.
/// </summary>
public interface ICustomerContextResolver
{
    /// <summary>
    /// Resolves customer identity and authentication state from the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">Incoming HTTP context.</param>
    /// <returns>Resolved customer context.</returns>
    CustomerContext Resolve(HttpContext httpContext);
}
