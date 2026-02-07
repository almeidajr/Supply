namespace Supply.Api.Domain.Options;

/// <summary>
/// Represents configuration options for customer policy options.
/// </summary>
public sealed class CustomerPolicyOptions
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
