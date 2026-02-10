using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Net.Http.Headers;
using Supply.Api.Application.Abstractions;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.RouteGroups;

internal static class WizardRoutes
{
    public static IEndpointRouteBuilder MapWizardRoutes(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/api/wizard").WithTags("Wizard").RequireRateLimiting("wizard-api");

        group
            .MapGet("/manifest", GetManifestAsync)
            .WithName("GetWizardManifest")
            .WithSummary("Get the resolved wizard manifest.")
            .WithDescription("Returns a wizard manifest for the requested channel and platform.")
            .Produces<WizardManifestDocument>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapGet("/binaries/{channel}/{operatingSystem}/{architecture}/latest", GetLatestWizardBinaryAsync)
            .WithName("GetLatestWizardBinary")
            .WithSummary("Get latest wizard binary metadata.")
            .WithDescription("Returns the latest wizard binary version and download URL for a channel and platform.")
            .Produces<WizardBinaryLatestDocument>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapGet("/binaries/{version}/{operatingSystem}/{architecture}", DownloadWizardBinaryAsync)
            .WithName("DownloadWizardBinary")
            .WithSummary("Download wizard binary.")
            .WithDescription("Streams a wizard binary file for the requested version and platform.")
            .Produces(statusCode: StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapMethods("/binaries/{version}/{operatingSystem}/{architecture}", ["HEAD"], HeadWizardBinaryAsync)
            .WithName("HeadWizardBinary")
            .WithSummary("Get wizard binary headers.")
            .WithDescription("Returns binary metadata headers without streaming file contents.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapGet("/artifacts/{artifactId}", DownloadArtifactAsync)
            .WithName("DownloadArtifact")
            .WithSummary("Download release artifact.")
            .WithDescription("Streams a release artifact file by artifact identifier.")
            .Produces(statusCode: StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapMethods("/artifacts/{artifactId}", ["HEAD"], HeadArtifactAsync)
            .WithName("HeadArtifact")
            .WithSummary("Get artifact headers.")
            .WithDescription("Returns artifact metadata headers without streaming file contents.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapPost("/download-tickets", CreateDownloadTicketsAsync)
            .WithName("CreateDownloadTickets")
            .WithSummary("Create signed download tickets.")
            .WithDescription("Issues short-lived signed URLs for one or more requested wizard artifacts or binaries.")
            .Accepts<DownloadTicketRequest>("application/json")
            .Produces<DownloadTicketResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }

    private static async Task<
        Results<Ok<WizardManifestDocument>, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > GetManifestAsync(
        [AsParameters] GetManifestQuery query,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardManifestService wizardManifestService,
        CancellationToken cancellationToken
    )
    {
        var customerContext = customerContextResolver.Resolve(httpContext);
        var baseUri = BuildBaseUri(httpContext);

        try
        {
            var envelope = await wizardManifestService.GetManifestAsync(
                new ManifestRequest
                {
                    Channel = query.Channel ?? "stable",
                    OperatingSystem = query.OperatingSystem,
                    Architecture = query.Architecture,
                    WizardVersion = query.WizardVersion,
                    BaseUri = baseUri,
                },
                customerContext,
                cancellationToken
            );

            if (envelope is null)
            {
                return TypedResults.NotFound();
            }

            if (MatchesETag(httpContext, envelope.ETag))
            {
                return TypedResults.StatusCode(StatusCodes.Status304NotModified);
            }

            httpContext.Response.Headers.ETag = envelope.ETag;
            httpContext.Response.Headers.CacheControl = "private, max-age=60, must-revalidate";
            return TypedResults.Ok(envelope.Document);
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

    private static async Task<
        Results<Ok<WizardBinaryLatestDocument>, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > GetLatestWizardBinaryAsync(
        [Description("Release channel to resolve.")] string channel,
        [Description("Target operating system identifier.")] string operatingSystem,
        [Description("Target CPU architecture identifier.")] string architecture,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardDistributionService wizardDistributionService,
        CancellationToken cancellationToken
    )
    {
        var customerContext = customerContextResolver.Resolve(httpContext);
        var baseUri = BuildBaseUri(httpContext);

        try
        {
            var latest = await wizardDistributionService.GetLatestWizardBinaryAsync(
                channel,
                operatingSystem,
                architecture,
                baseUri,
                customerContext,
                cancellationToken
            );

            if (latest is null)
            {
                return TypedResults.NotFound();
            }

            if (MatchesETag(httpContext, latest.ETag))
            {
                return TypedResults.StatusCode(StatusCodes.Status304NotModified);
            }

            httpContext.Response.Headers.ETag = latest.ETag;
            httpContext.Response.Headers.CacheControl = "public, max-age=30, stale-while-revalidate=30";
            return TypedResults.Ok(latest);
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

    private static Task<
        Results<FileStreamHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > DownloadWizardBinaryAsync(
        [Description("Wizard binary version to download.")] string version,
        [Description("Target operating system identifier.")] string operatingSystem,
        [Description("Target CPU architecture identifier.")] string architecture,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardDistributionService wizardDistributionService,
        CancellationToken cancellationToken
    )
    {
        return DownloadFileAsync(
            openFile: token =>
                wizardDistributionService.OpenWizardBinaryAsync(
                    version,
                    operatingSystem,
                    architecture,
                    customerContextResolver.Resolve(httpContext),
                    token
                ),
            httpContext,
            cancellationToken
        );
    }

    private static Task<
        Results<EmptyHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > HeadWizardBinaryAsync(
        [Description("Wizard binary version to inspect.")] string version,
        [Description("Target operating system identifier.")] string operatingSystem,
        [Description("Target CPU architecture identifier.")] string architecture,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardDistributionService wizardDistributionService,
        CancellationToken cancellationToken
    )
    {
        return HeadFileAsync(
            openFile: token =>
                wizardDistributionService.OpenWizardBinaryAsync(
                    version,
                    operatingSystem,
                    architecture,
                    customerContextResolver.Resolve(httpContext),
                    token
                ),
            httpContext,
            cancellationToken
        );
    }

    private static Task<
        Results<FileStreamHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > DownloadArtifactAsync(
        [Description("Artifact identifier to download.")] string artifactId,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardDistributionService wizardDistributionService,
        CancellationToken cancellationToken
    )
    {
        return DownloadFileAsync(
            openFile: token =>
                wizardDistributionService.OpenArtifactAsync(
                    artifactId,
                    customerContextResolver.Resolve(httpContext),
                    token
                ),
            httpContext,
            cancellationToken
        );
    }

    private static Task<Results<EmptyHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>> HeadArtifactAsync(
        [Description("Artifact identifier to inspect.")] string artifactId,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IWizardDistributionService wizardDistributionService,
        CancellationToken cancellationToken
    )
    {
        return HeadFileAsync(
            openFile: token =>
                wizardDistributionService.OpenArtifactAsync(
                    artifactId,
                    customerContextResolver.Resolve(httpContext),
                    token
                ),
            httpContext,
            cancellationToken
        );
    }

    private static async Task<Results<Ok<DownloadTicketResponse>, ProblemHttpResult>> CreateDownloadTicketsAsync(
        DownloadTicketRequest request,
        HttpContext httpContext,
        ICustomerContextResolver customerContextResolver,
        IDownloadTicketService downloadTicketService,
        CancellationToken cancellationToken
    )
    {
        if (request.Items.Count is 0)
        {
            return TypedResults.Problem(
                title: "Invalid request",
                detail: "At least one item must be provided.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var customerContext = customerContextResolver.Resolve(httpContext);
        try
        {
            var response = await downloadTicketService.CreateTicketsAsync(
                request,
                BuildBaseUri(httpContext),
                customerContext,
                cancellationToken
            );
            return TypedResults.Ok(response);
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

    private static async Task<
        Results<FileStreamHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > DownloadFileAsync(
        Func<CancellationToken, Task<ArtifactDownloadResult?>> openFile,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var artifact = await openFile(cancellationToken);
            if (artifact is null)
            {
                return TypedResults.NotFound();
            }

            if (MatchesETag(httpContext, artifact.ETag))
            {
                await artifact.DisposeAsync();
                return TypedResults.StatusCode(StatusCodes.Status304NotModified);
            }

            SetDownloadHeaders(httpContext, artifact);
            return TypedResults.File(
                artifact.ContentStream,
                contentType: artifact.ContentType,
                fileDownloadName: artifact.FileName,
                lastModified: artifact.LastModifiedUtc,
                entityTag: new EntityTagHeaderValue(artifact.ETag),
                enableRangeProcessing: true
            );
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

    private static async Task<
        Results<EmptyHttpResult, NotFound, StatusCodeHttpResult, ProblemHttpResult>
    > HeadFileAsync(
        Func<CancellationToken, Task<ArtifactDownloadResult?>> openFile,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var artifact = await openFile(cancellationToken);
            if (artifact is null)
            {
                return TypedResults.NotFound();
            }

            if (MatchesETag(httpContext, artifact.ETag))
            {
                await artifact.DisposeAsync();
                return TypedResults.StatusCode(StatusCodes.Status304NotModified);
            }

            SetDownloadHeaders(httpContext, artifact);
            await artifact.DisposeAsync();
            return TypedResults.Empty;
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

    private static void SetDownloadHeaders(HttpContext httpContext, ArtifactDownloadResult artifact)
    {
        httpContext.Response.Headers.ETag = artifact.ETag;
        httpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        httpContext.Response.Headers.AcceptRanges = "bytes";
        httpContext.Response.ContentType = artifact.ContentType;
        httpContext.Response.ContentLength = artifact.SizeBytes;
    }

    private static bool MatchesETag(HttpContext httpContext, string eTag)
    {
        if (!httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch))
        {
            return false;
        }

        var value = ifNoneMatch.ToString();
        return string.Equals(value, eTag, StringComparison.Ordinal)
            || value
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains(eTag, StringComparer.Ordinal);
    }

    private static string BuildBaseUri(HttpContext httpContext)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
    }
}

internal sealed record GetManifestQuery
{
    [Description("Target release channel. Defaults to 'stable' when omitted.")]
    public string? Channel { get; init; }

    [Description("Target operating system identifier.")]
    public string? OperatingSystem { get; init; }

    [Description("Target CPU architecture identifier.")]
    public string? Architecture { get; init; }

    [Description("Wizard version requesting the manifest.")]
    public string? WizardVersion { get; init; }
}
