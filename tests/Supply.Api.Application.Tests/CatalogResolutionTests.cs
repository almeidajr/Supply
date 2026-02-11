using Microsoft.AspNetCore.Http;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Application.Tests;

public sealed class CatalogResolutionTests
{
    [Fact]
    public void ResolveCustomerPolicy_WhenCatalogContainsCustomerPolicy_ShouldPreferCatalogPolicy()
    {
        var catalogPolicy = new CustomerPolicyDocument
        {
            AllowedChannels = ["stable"],
            PinnedReleaseByChannel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = "manifest-r1",
            },
        };
        var catalog = new CatalogDocument
        {
            CustomerPolicies = new Dictionary<string, CustomerPolicyDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["contoso"] = catalogPolicy,
            },
        };
        var options = new SupplyApiOptions
        {
            Customers = new Dictionary<string, CustomerPolicyOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["contoso"] = new CustomerPolicyOptions { AllowedChannels = ["beta"] },
            },
        };

        var resolved = CatalogResolution.ResolveCustomerPolicy(catalog, options, CreateCustomerContext());

        Assert.Same(catalogPolicy, resolved);
    }

    [Fact]
    public void ResolveCustomerPolicy_WhenDefinedInOptions_ShouldReturnMappedCopy()
    {
        var optionPolicy = new CustomerPolicyOptions
        {
            AllowedChannels = ["stable"],
            PinnedReleaseByChannel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = "manifest-r2",
            },
        };
        var catalog = new CatalogDocument();
        var options = new SupplyApiOptions
        {
            Customers = new Dictionary<string, CustomerPolicyOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["contoso"] = optionPolicy,
            },
        };

        var resolved = CatalogResolution.ResolveCustomerPolicy(catalog, options, CreateCustomerContext());

        Assert.Equal(["stable"], resolved.AllowedChannels);
        Assert.Equal("manifest-r2", resolved.PinnedReleaseByChannel["stable"]);
        resolved.AllowedChannels.Add("beta");
        resolved.PinnedReleaseByChannel["beta"] = "manifest-r3";
        Assert.DoesNotContain("beta", optionPolicy.AllowedChannels);
        Assert.False(optionPolicy.PinnedReleaseByChannel.ContainsKey("beta"));
    }

    [Fact]
    public void ResolveManifestReleaseId_WhenPinnedReleaseExists_ShouldReturnPinnedReleaseId()
    {
        var policy = new CustomerPolicyDocument
        {
            PinnedReleaseByChannel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = "manifest-r5",
            },
        };

        var releaseId = CatalogResolution.ResolveManifestReleaseId(new CatalogDocument(), "stable", policy);

        Assert.Equal("manifest-r5", releaseId);
    }

    [Fact]
    public void ResolveManifestReleaseId_WhenChannelDoesNotExist_ShouldThrowNotFound()
    {
        var exception = Assert.Throws<ApiRequestException>(() =>
            CatalogResolution.ResolveManifestReleaseId(new CatalogDocument(), "stable", new CustomerPolicyDocument())
        );

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public void ResolveWizardBinaryReleaseId_WhenPointerExists_ShouldReturnReleaseId()
    {
        var catalog = new CatalogDocument
        {
            ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = new ChannelPointerDocument
                {
                    Channel = "stable",
                    ManifestReleaseId = "manifest-r1",
                    WizardBinaryReleaseId = "wizard-r1",
                },
            },
        };

        var releaseId = CatalogResolution.ResolveWizardBinaryReleaseId(catalog, "stable", new CustomerPolicyDocument());

        Assert.Equal("wizard-r1", releaseId);
    }

    [Fact]
    public void ValidateAccess_WhenAuthenticationIsRequiredAndContextIsAnonymous_ShouldThrowUnauthorized()
    {
        var options = new SupplyApiOptions { RequireAuthentication = true };
        var context = new CustomerContext { CustomerId = "contoso", IsAuthenticated = false };

        var exception = Assert.Throws<ApiRequestException>(() =>
            CatalogResolution.ValidateAccess(options, context, "stable", new CustomerPolicyDocument())
        );

        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact]
    public void ValidateAccess_WhenChannelIsNotAllowed_ShouldThrowForbidden()
    {
        var options = new SupplyApiOptions();
        var context = CreateCustomerContext();
        var policy = new CustomerPolicyDocument { AllowedChannels = ["beta"] };

        var exception = Assert.Throws<ApiRequestException>(() =>
            CatalogResolution.ValidateAccess(options, context, "stable", policy)
        );

        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact]
    public void ValidateAccess_WhenAllowedChannelsIsEmpty_ShouldAllowAnyChannel()
    {
        var exception = Record.Exception(() =>
            CatalogResolution.ValidateAccess(
                new SupplyApiOptions(),
                CreateCustomerContext(),
                "stable",
                new CustomerPolicyDocument()
            )
        );

        Assert.Null(exception);
    }

    private static CustomerContext CreateCustomerContext() => new() { CustomerId = "contoso", IsAuthenticated = true };
}
