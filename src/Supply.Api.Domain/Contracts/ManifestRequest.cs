namespace Supply.Api.Domain.Contracts;

public sealed record ManifestRequest
{
    public required string Channel { get; init; }

    public string? OperatingSystem { get; init; }

    public string? Architecture { get; init; }

    public string? WizardVersion { get; init; }

    public string? BaseUri { get; init; }
}
