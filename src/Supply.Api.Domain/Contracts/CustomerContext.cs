namespace Supply.Api.Domain.Contracts;

/// <summary>
/// Represents the customer context.
/// </summary>
public sealed record CustomerContext
{
    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    public required string CustomerId { get; init; }

    /// <summary>
    /// Gets or sets the is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; init; }
}
