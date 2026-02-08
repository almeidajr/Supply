using Supply.Wizard.Application;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Domain;
using Supply.Wizard.Infrastructure.Http;

namespace Supply.Wizard.Infrastructure.Downloads;

/// <summary>
/// Downloads component artifacts into a local cache.
/// </summary>
public sealed class ArtifactDownloader(HttpClient httpClient) : IArtifactDownloader
{
    public async Task<ArtifactDownloadResult> DownloadAsync(
        ArtifactManifest artifact,
        DownloadContext context,
        CancellationToken cancellationToken
    )
    {
        Directory.CreateDirectory(context.CacheDirectoryPath);
        var cacheFilePath = BuildCachePath(artifact, context.CacheDirectoryPath);

        if (File.Exists(cacheFilePath))
        {
            return new ArtifactDownloadResult { FilePath = cacheFilePath, ReusedCachedFile = true };
        }

        var tempFilePath = $"{cacheFilePath}.tmp";

        try
        {
            if (HttpClientBuilder.RequiresDedicatedTransport(context.Authentication, context.Tls))
            {
                using var dedicatedClient = await HttpClientBuilder.CreateAsync(
                    context.Authentication,
                    context.Tls,
                    cancellationToken
                );
                using var dedicatedResponse = await dedicatedClient.GetAsync(
                    artifact.DownloadUri,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                );
                await SaveArtifactAsync(dedicatedResponse, tempFilePath, cancellationToken);
            }
            else
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, artifact.DownloadUri);
                await HttpClientBuilder.ApplyAuthenticationAsync(
                    request.Headers,
                    context.Authentication,
                    cancellationToken
                );
                using var response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                );
                await SaveArtifactAsync(response, tempFilePath, cancellationToken);
            }
        }
        catch (ArtifactIntegrityException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ArtifactIntegrityException(
                $"Failed to download artifact '{artifact.FileName}' from '{artifact.DownloadUri}'.",
                exception
            );
        }

        File.Move(tempFilePath, cacheFilePath, overwrite: true);
        return new ArtifactDownloadResult { FilePath = cacheFilePath, ReusedCachedFile = false };
    }

    private static string BuildCachePath(ArtifactManifest artifact, string cacheDirectoryPath)
    {
        var safeFileName = Path.GetFileName(artifact.FileName);
        var cacheFileName = $"{artifact.Sha256}_{safeFileName}";
        return Path.Combine(cacheDirectoryPath, cacheFileName);
    }

    private static async Task SaveArtifactAsync(
        HttpResponseMessage response,
        string tempFilePath,
        CancellationToken cancellationToken
    )
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ArtifactIntegrityException(
                $"Artifact download failed with status {(int)response.StatusCode} ({response.ReasonPhrase ?? "unknown"})."
            );
        }

        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destinationStream = File.Create(tempFilePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }
}
