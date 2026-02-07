namespace Supply.Api.Domain.Options;

public sealed class CustomerPolicyOptions
{
    public List<string> AllowedChannels { get; init; } = [];

    public Dictionary<string, string> PinnedReleaseByChannel { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
