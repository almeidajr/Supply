using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Infrastructure.Security;

/// <summary>
/// Resolves customer context from claims first, then request headers, with a public fallback.
/// </summary>
public sealed class HeaderOrTokenCustomerContextResolver : ICustomerContextResolver
{
    private const string CustomerIdHeaderName = "X-Supply-Customer-Id";

    /// <summary>
    /// Resolves the current customer id and authentication state from the HTTP context.
    /// </summary>
    /// <param name="httpContext">Incoming request context.</param>
    /// <returns>Resolved customer context for the request.</returns>
    public CustomerContext Resolve(HttpContext httpContext)
    {
        var customerId =
            TryResolveFromClaims(httpContext.User) ?? TryResolveFromHeader(httpContext.Request.Headers) ?? "public";

        return new CustomerContext
        {
            CustomerId = customerId,
            IsAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false,
        };
    }

    private static string? TryResolveFromClaims(ClaimsPrincipal principal) =>
        principal.FindFirst("customer_id")?.Value
        ?? principal.FindFirst("customerId")?.Value
        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static string? TryResolveFromHeader(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(CustomerIdHeaderName, out var value))
        {
            return null;
        }

        var resolved = value.ToString();
        return string.IsNullOrWhiteSpace(resolved) ? null : resolved.Trim();
    }
}
