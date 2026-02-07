namespace Supply.Api.Domain.Catalog;

public sealed record CustomerPolicyDocument
{
    public List<string> AllowedChannels { get; init; } = [];

    public Dictionary<string, string> PinnedReleaseByChannel { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
