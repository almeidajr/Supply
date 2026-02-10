using Serilog;
using Supply.Edge.Collector;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog(
    (services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
