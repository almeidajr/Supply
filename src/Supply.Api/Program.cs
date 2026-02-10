using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Serilog;
using Supply.Api.Application.Abstractions;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;
using Supply.Api.Filters;
using Supply.Api.Infrastructure.Catalog;
using Supply.Api.Infrastructure.Health;
using Supply.Api.Infrastructure.Security;
using Supply.Api.Infrastructure.Storage;
using Supply.Api.RouteGroups;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
);

builder.Services.Configure<SupplyApiOptions>(
    builder.Configuration.GetSection(SupplyApiOptions.ConfigurationSectionName)
);

builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Supply API",
                Version = "v1",
                Description = "Supply distribution and control plane APIs for wizard manifest and artifact delivery.",
            };

            document.Servers =
            [
                new OpenApiServer { Url = "/", Description = "Relative URL for the currently addressed host." },
            ];

            document.Tags = new HashSet<OpenApiTag>
            {
                new()
                {
                    Name = "Wizard",
                    Description = "Wizard-facing endpoints for manifests, binaries, artifacts, and download tickets.",
                },
                new()
                {
                    Name = "Internal Releases",
                    Description = "Internal release administration endpoints for publishing release channels.",
                },
                new() { Name = "Health", Description = "Service liveness and readiness probes." },
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(
                StringComparer.Ordinal
            );
            document.Components.SecuritySchemes["internalApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Supply-Internal-Key",
                Description = "Internal API key required by release administration endpoints.",
            };

            return Task.CompletedTask;
        }
    );

    options.AddSchemaTransformer(
        (schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type == typeof(DownloadTicketRequest))
            {
                schema.Description ??=
                    "Request payload for generating short-lived signed download URLs for wizard artifacts and binaries.";
            }

            return Task.CompletedTask;
        }
    );
});
builder
    .Services.AddHealthChecks()
    .AddCheck<ArtifactStorageHealthCheck>("artifacts")
    .AddCheck<CatalogHealthCheck>("catalog");

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(
        "wizard-api",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 120;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );
});

builder.Services.AddSingleton<ICustomerContextResolver, HeaderOrTokenCustomerContextResolver>();
builder.Services.AddSingleton<IArtifactStorage, FileSystemArtifactStorage>();
builder.Services.AddSingleton<IReleaseCatalogRepository, JsonReleaseCatalogRepository>();
builder.Services.AddSingleton<IWizardManifestService, WizardManifestService>();
builder.Services.AddSingleton<IWizardDistributionService, WizardDistributionService>();
builder.Services.AddSingleton<IDownloadTicketService, DownloadTicketService>();
builder.Services.AddSingleton<IReleaseAdministrationService, ReleaseAdministrationService>();
builder.Services.AddSingleton<ArtifactStorageHealthCheck>();
builder.Services.AddSingleton<CatalogHealthCheck>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();
app.UseRateLimiter();

app.MapOpenApi();
app.MapHealthRoutes();
app.MapWizardRoutes();
app.MapInternalReleaseRoutes().AddEndpointFilter<InternalApiKeyEndpointFilter>();

await app.RunAsync();
