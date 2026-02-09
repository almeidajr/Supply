using System.Text.Json;
using Supply.Wizard.Application;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Infrastructure.Http;

/// <summary>
/// HTTP manifest client backed by Supply.Api.
/// </summary>
public sealed class HttpManifestClient(HttpClient httpClient) : IManifestClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Fetches the wizard manifest from Supply.Api for the requested channel.
    /// </summary>
    /// <param name="query">Manifest query containing channel and transport/auth options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved manifest document.</returns>
    public async Task<ManifestDocument> GetManifestAsync(ManifestQuery query, CancellationToken cancellationToken)
    {
        var manifestUri = new Uri(
            query.ApiBaseUri,
            $"/api/wizard/manifest?channel={Uri.EscapeDataString(query.Channel)}"
        );

        try
        {
            if (HttpClientBuilder.RequiresDedicatedTransport(query.Authentication, query.Tls))
            {
                using var dedicatedClient = await HttpClientBuilder.CreateAsync(
                    query.Authentication,
                    query.Tls,
                    cancellationToken
                );
                using var dedicatedResponse = await dedicatedClient.GetAsync(manifestUri, cancellationToken);
                return await ParseManifestAsync(dedicatedResponse, cancellationToken);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, manifestUri);
            await HttpClientBuilder.ApplyAuthenticationAsync(request.Headers, query.Authentication, cancellationToken);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            return await ParseManifestAsync(response, cancellationToken);
        }
        catch (ApiAccessException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ApiAccessException("Failed to retrieve manifest from Supply.Api.", exception);
        }
    }

    private static async Task<ManifestDocument> ParseManifestAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiAccessException(
                $"Manifest request failed with status {(int)response.StatusCode} ({response.ReasonPhrase ?? "unknown"})."
            );
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var manifest = await JsonSerializer.DeserializeAsync<ManifestDocument>(
            contentStream,
            SerializerOptions,
            cancellationToken
        );

        return manifest ?? throw new ApiAccessException("Manifest response was empty or invalid.");
    }
}
