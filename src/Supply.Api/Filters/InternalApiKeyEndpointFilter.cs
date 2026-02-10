using Microsoft.Extensions.Options;
using Supply.Api.Domain.Options;

namespace Supply.Api.Filters;

internal sealed class InternalApiKeyEndpointFilter(IOptions<SupplyApiOptions> options) : IEndpointFilter
{
    private readonly SupplyApiOptions _supplyApiOptions = options.Value;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_supplyApiOptions.InternalApiKey))
        {
            return await next(context);
        }

        var httpContext = context.HttpContext;
        if (!httpContext.Request.Headers.TryGetValue("X-Supply-Internal-Key", out var apiKeyHeader))
        {
            return Results.Problem(
                detail: "Missing internal API key.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized"
            );
        }

        if (!string.Equals(apiKeyHeader.ToString(), _supplyApiOptions.InternalApiKey, StringComparison.Ordinal))
        {
            return Results.Problem(
                detail: "Invalid internal API key.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden"
            );
        }

        return await next(context);
    }
}
