using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

public interface IWizardDistributionService
{
    Task<WizardBinaryLatestDocument?> GetLatestWizardBinaryAsync(
        string channel,
        string operatingSystem,
        string architecture,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );

    Task<ArtifactDownloadResult?> OpenWizardBinaryAsync(
        string version,
        string operatingSystem,
        string architecture,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );

    Task<ArtifactDownloadResult?> OpenArtifactAsync(
        string artifactId,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    );
}
