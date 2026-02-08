using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

public interface IWizardManifestService
{
    Task<ManifestEnvelope?> GetManifestAsync(
        ManifestRequest request,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
