using Supply.Api.Application.Abstractions;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Services;

public sealed class DownloadTicketService(IWizardDistributionService wizardDistributionService) : IDownloadTicketService
{
    public async Task<DownloadTicketResponse> CreateTicketsAsync(
        DownloadTicketRequest request,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(request.TimeToLiveSeconds, 30, 900));
        var links = new List<DownloadTicketLinkDocument>();

        foreach (var item in request.Items)
        {
            var url = await ResolveUrlAsync(item, baseUri, customerContext, cancellationToken);
            if (url is null)
            {
                continue;
            }

            links.Add(new DownloadTicketLinkDocument { Id = item.Id, Url = url });
        }

        return new DownloadTicketResponse { ExpiresAtUtc = expiresAtUtc, Links = links };
    }

    private async Task<Uri?> ResolveUrlAsync(
        DownloadTicketItemRequest item,
        string? baseUri,
        CustomerContext customerContext,
        CancellationToken cancellationToken
    )
    {
        var resolvedBaseUri = ResolveBaseUri(baseUri);
        if (string.Equals(item.Type, "artifact", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(resolvedBaseUri, $"/api/wizard/artifacts/{Uri.EscapeDataString(item.Id)}");
        }

        if (string.Equals(item.Type, "wizardBinary", StringComparison.OrdinalIgnoreCase))
        {
            var parts = item.Id.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is not 3)
            {
                return null;
            }

            var latest = await wizardDistributionService.GetLatestWizardBinaryAsync(
                parts[0],
                parts[1],
                parts[2],
                baseUri,
                customerContext,
                cancellationToken
            );

            return latest?.DownloadUri;
        }

        return null;
    }

    private static Uri ResolveBaseUri(string? baseUri)
    {
        if (!string.IsNullOrWhiteSpace(baseUri) && Uri.TryCreate(baseUri, UriKind.Absolute, out var parsed))
        {
            return parsed;
        }

        return new UriBuilder(Uri.UriSchemeHttps, "localhost", 5001).Uri;
    }
}
