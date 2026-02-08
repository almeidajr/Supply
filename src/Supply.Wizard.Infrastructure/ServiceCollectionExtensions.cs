using Microsoft.Extensions.DependencyInjection;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Infrastructure.Downloads;
using Supply.Wizard.Infrastructure.Http;

namespace Supply.Wizard.Infrastructure;

/// <summary>
/// Service registration helpers for wizard infrastructure components.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers factory-backed typed HTTP clients used by wizard infrastructure adapters.
        /// </summary>
        /// <returns>The same service collection instance.</returns>
        public IServiceCollection AddWizardInfrastructureHttpClients()
        {
            services.AddHttpClient<IManifestClient, HttpManifestClient>(client =>
                client.Timeout = TimeSpan.FromSeconds(60)
            );
            services.AddHttpClient<IArtifactDownloader, ArtifactDownloader>(client =>
                client.Timeout = TimeSpan.FromSeconds(60)
            );

            return services;
        }
    }
}
