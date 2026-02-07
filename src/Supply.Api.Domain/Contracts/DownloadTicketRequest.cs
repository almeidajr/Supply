namespace Supply.Api.Domain.Contracts;

public sealed record DownloadTicketRequest
{
    public IReadOnlyList<DownloadTicketItemRequest> Items { get; init; } = [];

    public int TimeToLiveSeconds { get; init; } = 300;
}
