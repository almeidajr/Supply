using Microsoft.AspNetCore.Http;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Services;

public static class CatalogResolution
{
    public static CustomerPolicyDocument ResolveCustomerPolicy(
        CatalogDocument catalog,
        SupplyApiOptions options,
        CustomerContext customerContext
    )
    {
        if (catalog.CustomerPolicies.TryGetValue(customerContext.CustomerId, out var catalogPolicy))
        {
            return catalogPolicy;
        }

        if (!options.Customers.TryGetValue(customerContext.CustomerId, out var optionPolicy))
        {
            return new CustomerPolicyDocument();
        }

        return new CustomerPolicyDocument
        {
            AllowedChannels = [.. optionPolicy.AllowedChannels],
            PinnedReleaseByChannel = new Dictionary<string, string>(
                optionPolicy.PinnedReleaseByChannel,
                StringComparer.OrdinalIgnoreCase
            ),
        };
    }

    public static string ResolveManifestReleaseId(
        CatalogDocument catalog,
        string channel,
        CustomerPolicyDocument policy
    )
    {
        if (policy.PinnedReleaseByChannel.TryGetValue(channel, out var pinnedReleaseId))
        {
            return pinnedReleaseId;
        }

        if (!catalog.ChannelPointers.TryGetValue(channel, out var channelPointer))
        {
            throw new ApiRequestException($"Channel '{channel}' is not configured.", StatusCodes.Status404NotFound);
        }

        return channelPointer.ManifestReleaseId;
    }

    public static string ResolveWizardBinaryReleaseId(
        CatalogDocument catalog,
        string channel,
        CustomerPolicyDocument policy
    )
    {
        if (policy.PinnedReleaseByChannel.TryGetValue(channel, out var pinnedReleaseId))
        {
            return pinnedReleaseId;
        }

        if (!catalog.ChannelPointers.TryGetValue(channel, out var channelPointer))
        {
            throw new ApiRequestException($"Channel '{channel}' is not configured.", StatusCodes.Status404NotFound);
        }

        return channelPointer.WizardBinaryReleaseId;
    }

    public static void ValidateAccess(
        SupplyApiOptions options,
        CustomerContext customerContext,
        string channel,
        CustomerPolicyDocument policy
    )
    {
        if (options.RequireAuthentication && !customerContext.IsAuthenticated)
        {
            throw new ApiRequestException("Authentication is required.", StatusCodes.Status401Unauthorized);
        }

        if (policy.AllowedChannels.Count is 0)
        {
            return;
        }

        var allowed = policy.AllowedChannels.Contains(channel, StringComparer.OrdinalIgnoreCase);
        if (!allowed)
        {
            throw new ApiRequestException(
                $"Customer '{customerContext.CustomerId}' is not authorized for channel '{channel}'.",
                StatusCodes.Status403Forbidden
            );
        }
    }
}
