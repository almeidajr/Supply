using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Supply.Api.Application.Abstractions;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.RouteGroups;

internal static class InternalReleaseRoutes
{
    public static RouteGroupBuilder MapInternalReleaseRoutes(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder
            .MapGroup("/api/internal/releases")
            .WithTags("Internal Releases")
            .RequireRateLimiting("wizard-api");

        group
            .MapPost("/", UpsertReleaseAsync)
            .WithName("UpsertInternalRelease")
            .WithSummary("Upsert release payloads.")
            .WithDescription(
                "Upserts manifest release, wizard binary release, and artifact metadata in a single request."
            )
            .Accepts<InternalUpsertReleaseRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapPost("/{releaseId}/publish", PublishReleaseAsync)
            .WithName("PublishInternalReleaseChannel")
            .WithSummary("Publish release channel pointers.")
            .WithDescription("Updates channel pointers to reference a manifest release and/or wizard binary release.")
            .Accepts<InternalPublishChannelRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<Results<Accepted, ProblemHttpResult>> UpsertReleaseAsync(
        InternalUpsertReleaseRequest request,
        IReleaseAdministrationService releaseAdministrationService,
        CancellationToken cancellationToken
    )
    {
        if (request.ManifestRelease is null && request.WizardBinaryRelease is null && request.Artifacts.Count is 0)
        {
            return TypedResults.Problem(
                title: "Invalid request",
                detail: "At least one release payload must be provided.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        try
        {
            await releaseAdministrationService.UpsertReleaseAsync(request, cancellationToken);
            return TypedResults.Accepted((string?)null);
        }
        catch (ApiRequestException exception)
        {
            return TypedResults.Problem(
                title: "Request failed",
                detail: exception.Message,
                statusCode: exception.StatusCode
            );
        }
    }

    private static async Task<Results<Accepted, ProblemHttpResult>> PublishReleaseAsync(
        [Description("Release identifier to publish.")] string releaseId,
        InternalPublishChannelRequest request,
        IReleaseAdministrationService releaseAdministrationService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await releaseAdministrationService.PublishChannelAsync(releaseId, request, cancellationToken);
            return TypedResults.Accepted((string?)null);
        }
        catch (ApiRequestException exception)
        {
            return TypedResults.Problem(
                title: "Request failed",
                detail: exception.Message,
                statusCode: exception.StatusCode
            );
        }
    }
}
