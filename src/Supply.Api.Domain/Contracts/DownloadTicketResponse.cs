namespace Supply.Api.Domain.Contracts;

public sealed record DownloadTicketResponse
{
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public IReadOnlyList<DownloadTicketLinkDocument> Links { get; init; } = [];
}
