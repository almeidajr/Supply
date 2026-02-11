using Supply.Api.Application.Services;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Tests;

public sealed class DownloadTicketServiceTests
{
    [Fact]
    public async Task CreateTicketsAsync_WhenItemsAreSupported_ShouldReturnResolvedLinks()
    {
        var distributionService = new FakeWizardDistributionService();
        distributionService.SetLatestWizardBinary(
            "stable",
            "windows",
            "x64",
            new WizardBinaryLatestDocument
            {
                Channel = "stable",
                Version = "1.2.3",
                Os = "windows",
                Architecture = "x64",
                SizeBytes = 1,
                Sha256 = "abc123",
                DownloadUri = new Uri("https://downloads.example.com/wizard.zip"),
                ETag = "\"sha256:abc123\"",
                PublishedAtUtc = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            }
        );
        var service = new DownloadTicketService(distributionService);
        var request = new DownloadTicketRequest
        {
            Items =
            [
                new DownloadTicketItemRequest { Type = "artifact", Id = "artifact-win-x64" },
                new DownloadTicketItemRequest { Type = "wizardBinary", Id = "stable/windows/x64" },
                new DownloadTicketItemRequest { Type = "wizardBinary", Id = "stable/windows" },
                new DownloadTicketItemRequest { Type = "unsupported", Id = "ignored" },
            ],
        };

        var response = await service.CreateTicketsAsync(
            request,
            "https://api.example.com/",
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.Equal(2, response.Links.Count);
        var artifactLink = Assert.Single(response.Links, link => link.Id == "artifact-win-x64");
        var wizardLink = Assert.Single(response.Links, link => link.Id == "stable/windows/x64");
        Assert.Equal(new Uri("https://api.example.com/api/wizard/artifacts/artifact-win-x64"), artifactLink.Url);
        Assert.Equal(new Uri("https://downloads.example.com/wizard.zip"), wizardLink.Url);

        var latestRequest = Assert.Single(distributionService.LatestRequests);
        Assert.Equal("stable", latestRequest.Channel);
        Assert.Equal("windows", latestRequest.OperatingSystem);
        Assert.Equal("x64", latestRequest.Architecture);
    }

    [Fact]
    public async Task CreateTicketsAsync_WhenBaseUriIsInvalid_ShouldFallbackToLocalhost()
    {
        var service = new DownloadTicketService(new FakeWizardDistributionService());
        var request = new DownloadTicketRequest
        {
            Items = [new DownloadTicketItemRequest { Type = "artifact", Id = "artifact-a" }],
        };

        var response = await service.CreateTicketsAsync(
            request,
            "not-a-valid-uri",
            CreateCustomerContext(),
            CancellationToken.None
        );

        var link = Assert.Single(response.Links);
        Assert.Equal("https://localhost:5001/api/wizard/artifacts/artifact-a", link.Url.ToString());
    }

    [Fact]
    public async Task CreateTicketsAsync_WhenWizardBinaryCannotBeResolved_ShouldSkipLink()
    {
        var service = new DownloadTicketService(new FakeWizardDistributionService());
        var request = new DownloadTicketRequest
        {
            Items = [new DownloadTicketItemRequest { Type = "wizardBinary", Id = "stable/windows/x64" }],
        };

        var response = await service.CreateTicketsAsync(
            request,
            "https://api.example.com/",
            CreateCustomerContext(),
            CancellationToken.None
        );

        Assert.Empty(response.Links);
    }

    [Theory]
    [InlineData(5, 30)]
    [InlineData(5000, 900)]
    public async Task CreateTicketsAsync_WhenRequestedTimeToLiveIsOutOfRange_ShouldClamp(
        int requestedTimeToLiveSeconds,
        int expectedTimeToLiveSeconds
    )
    {
        var service = new DownloadTicketService(new FakeWizardDistributionService());
        var request = new DownloadTicketRequest { TimeToLiveSeconds = requestedTimeToLiveSeconds };
        var before = DateTimeOffset.UtcNow;

        var response = await service.CreateTicketsAsync(
            request,
            "https://api.example.com/",
            CreateCustomerContext(),
            CancellationToken.None
        );

        var after = DateTimeOffset.UtcNow;
        Assert.InRange(
            response.ExpiresAtUtc,
            before.AddSeconds(expectedTimeToLiveSeconds),
            after.AddSeconds(expectedTimeToLiveSeconds)
        );
    }

    private static CustomerContext CreateCustomerContext() => new() { CustomerId = "contoso", IsAuthenticated = true };
}
