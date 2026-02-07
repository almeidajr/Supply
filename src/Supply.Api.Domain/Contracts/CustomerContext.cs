namespace Supply.Api.Domain.Contracts;

public sealed record CustomerContext
{
    public required string CustomerId { get; init; }

    public bool IsAuthenticated { get; init; }
}
