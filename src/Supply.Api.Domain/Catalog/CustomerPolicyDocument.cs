namespace Supply.Api.Domain.Catalog;

/// <summary>
/// Represents the customer policy document.
/// </summary>
public sealed record CustomerPolicyDocument
{
    /// <summary>
    /// Gets or sets the allowed channels.
    /// </summary>
    public List<string> AllowedChannels { get; init; } = [];

    /// <summary>
    /// Gets or sets pinned release identifiers by channel name.
    /// </summary>
    public Dictionary<string, string> PinnedReleaseByChannel { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
