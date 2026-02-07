namespace Supply.Api.Domain.Contracts;

public sealed record DownloadTicketLinkDocument
{
    public required string Id { get; init; }

    public required Uri Url { get; init; }
}
