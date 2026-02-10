using System.ComponentModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Supply.Api.RouteGroups;

internal static class HealthRoutes
{
    public static IEndpointRouteBuilder MapHealthRoutes(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/health/live", GetLiveness)
            .WithName("GetHealthLiveness")
            .WithSummary("Get liveness state.")
            .WithDescription("Returns a lightweight liveness signal for process health.")
            .WithTags("Health")
            .Produces<HealthStatusDocument>(StatusCodes.Status200OK, "application/json");

        routeBuilder
            .MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = static _ => true,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                    },
                }
            )
            .WithName("GetHealthReadiness")
            .WithSummary("Get readiness state.")
            .WithDescription("Returns readiness status based on storage and catalog health checks.")
            .WithTags("Health");

        return routeBuilder;
    }

    private static Ok<HealthStatusDocument> GetLiveness()
    {
        return TypedResults.Ok(new HealthStatusDocument("live"));
    }
}

internal sealed record HealthStatusDocument([property: Description("Health status value.")] string Status);
