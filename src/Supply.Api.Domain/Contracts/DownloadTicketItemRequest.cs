namespace Supply.Api.Domain.Contracts;

public sealed record DownloadTicketItemRequest
{
    public required string Type { get; init; }

    public required string Id { get; init; }
}
