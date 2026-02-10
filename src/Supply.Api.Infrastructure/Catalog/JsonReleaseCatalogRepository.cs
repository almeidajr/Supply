using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Supply.Api.Application.Abstractions;
using Supply.Api.Application.Services;
using Supply.Api.Domain.Catalog;
using Supply.Api.Domain.Contracts;
using Supply.Api.Domain.Options;

namespace Supply.Api.Infrastructure.Catalog;

/// <summary>
/// Stores release catalog data in a JSON file on disk.
/// </summary>
/// <param name="options">Resolved API options used to locate repository and catalog files.</param>
public sealed class JsonReleaseCatalogRepository(IOptions<SupplyApiOptions> options)
    : IReleaseCatalogRepository,
        IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly SupplyApiOptions _supplyApiOptions = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Loads the current catalog document from storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>The current catalog document.</returns>
    public async Task<CatalogDocument> GetCatalogAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCatalogFileExistsAsync(cancellationToken);
            return await LoadCatalogUnsafeAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Inserts or updates release entities and artifacts in the catalog.
    /// </summary>
    /// <param name="request">Release payload to upsert.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    public async Task UpsertReleaseAsync(UpsertReleaseRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCatalogFileExistsAsync(cancellationToken);
            var catalog = await LoadCatalogUnsafeAsync(cancellationToken);

            foreach (var artifact in request.Artifacts)
            {
                catalog.Artifacts[artifact.Id] = artifact;
            }

            if (request.ManifestRelease is not null)
            {
                catalog.ManifestReleases[request.ManifestRelease.Id] = request.ManifestRelease;
            }

            if (request.WizardBinaryRelease is not null)
            {
                catalog.WizardBinaryReleases[request.WizardBinaryRelease.Id] = request.WizardBinaryRelease;
            }

            await SaveCatalogUnsafeAsync(catalog, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Updates the channel pointer to publish manifest and wizard binary releases.
    /// </summary>
    /// <param name="request">Publish request containing target channel and release ids.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    public async Task PublishChannelPointerAsync(PublishReleaseRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCatalogFileExistsAsync(cancellationToken);
            var catalog = await LoadCatalogUnsafeAsync(cancellationToken);

            if (!catalog.ChannelPointers.TryGetValue(request.Channel, out var current))
            {
                var manifestReleaseId =
                    request.ManifestReleaseId
                    ?? throw new ApiRequestException(
                        $"Channel '{request.Channel}' is missing a manifest release id.",
                        StatusCodes.Status400BadRequest
                    );
                var wizardBinaryReleaseId =
                    request.WizardBinaryReleaseId
                    ?? throw new ApiRequestException(
                        $"Channel '{request.Channel}' is missing a wizard release id.",
                        StatusCodes.Status400BadRequest
                    );

                catalog.ChannelPointers[request.Channel] = new ChannelPointerDocument
                {
                    Channel = request.Channel,
                    ManifestReleaseId = manifestReleaseId,
                    WizardBinaryReleaseId = wizardBinaryReleaseId,
                };

                await SaveCatalogUnsafeAsync(catalog, cancellationToken);
                return;
            }

            catalog.ChannelPointers[request.Channel] = current with
            {
                ManifestReleaseId = request.ManifestReleaseId ?? current.ManifestReleaseId,
                WizardBinaryReleaseId = request.WizardBinaryReleaseId ?? current.WizardBinaryReleaseId,
            };

            await SaveCatalogUnsafeAsync(catalog, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Releases repository synchronization resources.
    /// </summary>
    public void Dispose()
    {
        _gate.Dispose();
    }

    private async Task EnsureCatalogFileExistsAsync(CancellationToken cancellationToken)
    {
        var directoryPath = Path.GetDirectoryName(_supplyApiOptions.CatalogFilePath);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            directoryPath = _supplyApiOptions.RepositoryRootPath;
        }

        Directory.CreateDirectory(directoryPath);

        if (File.Exists(_supplyApiOptions.CatalogFilePath))
        {
            return;
        }

        var seedCatalog = BuildSeedCatalog();
        await SaveCatalogUnsafeAsync(seedCatalog, cancellationToken);
    }

    private async Task<CatalogDocument> LoadCatalogUnsafeAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(_supplyApiOptions.CatalogFilePath);
        var catalog = await JsonSerializer.DeserializeAsync<CatalogDocument>(
            stream,
            SerializerOptions,
            cancellationToken
        );

        return catalog ?? new CatalogDocument();
    }

    private async Task SaveCatalogUnsafeAsync(CatalogDocument catalog, CancellationToken cancellationToken)
    {
        var temporaryFilePath = _supplyApiOptions.CatalogFilePath + ".tmp";
        await using (var stream = File.Create(temporaryFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, catalog, SerializerOptions, cancellationToken);
        }

        File.Move(temporaryFilePath, _supplyApiOptions.CatalogFilePath, overwrite: true);
    }

    private static CatalogDocument BuildSeedCatalog()
    {
        var wizardWindowsHash = ManifestHashHelper.ComputeSha256("wizard-1.0.0-windows-x64");
        var wizardLinuxHash = ManifestHashHelper.ComputeSha256("wizard-1.0.0-linux-x64");
        var agentWindowsHash = ManifestHashHelper.ComputeSha256("agent-1.0.0-windows-x64");
        var agentLinuxHash = ManifestHashHelper.ComputeSha256("agent-1.0.0-linux-x64");
        var collectorWindowsHash = ManifestHashHelper.ComputeSha256("collector-1.0.0-windows-x64");
        var collectorLinuxHash = ManifestHashHelper.ComputeSha256("collector-1.0.0-linux-x64");
        var processorWindowsHash = ManifestHashHelper.ComputeSha256("processor-1.0.0-windows-x64");
        var processorLinuxHash = ManifestHashHelper.ComputeSha256("processor-1.0.0-linux-x64");

        var artifacts = new Dictionary<string, ArtifactDocument>(StringComparer.OrdinalIgnoreCase)
        {
            ["wizard_1.0.0_windows_x64"] = CreateArtifact(
                "wizard_1.0.0_windows_x64",
                Path.Combine("wizard", "1.0.0", "windows-x64", "supply-wizard.zip"),
                "supply-wizard.zip",
                wizardWindowsHash,
                "windows",
                "x64"
            ),
            ["wizard_1.0.0_linux_x64"] = CreateArtifact(
                "wizard_1.0.0_linux_x64",
                Path.Combine("wizard", "1.0.0", "linux-x64", "supply-wizard.tar.gz"),
                "supply-wizard.tar.gz",
                wizardLinuxHash,
                "linux",
                "x64"
            ),
            ["edge_agent_1.0.0_windows_x64"] = CreateArtifact(
                "edge_agent_1.0.0_windows_x64",
                Path.Combine("edge", "agent", "1.0.0", "windows-x64", "supply-edge-agent.zip"),
                "supply-edge-agent.zip",
                agentWindowsHash,
                "windows",
                "x64"
            ),
            ["edge_agent_1.0.0_linux_x64"] = CreateArtifact(
                "edge_agent_1.0.0_linux_x64",
                Path.Combine("edge", "agent", "1.0.0", "linux-x64", "supply-edge-agent.tar.gz"),
                "supply-edge-agent.tar.gz",
                agentLinuxHash,
                "linux",
                "x64"
            ),
            ["edge_collector_1.0.0_windows_x64"] = CreateArtifact(
                "edge_collector_1.0.0_windows_x64",
                Path.Combine("edge", "collector", "1.0.0", "windows-x64", "supply-edge-collector.zip"),
                "supply-edge-collector.zip",
                collectorWindowsHash,
                "windows",
                "x64"
            ),
            ["edge_collector_1.0.0_linux_x64"] = CreateArtifact(
                "edge_collector_1.0.0_linux_x64",
                Path.Combine("edge", "collector", "1.0.0", "linux-x64", "supply-edge-collector.tar.gz"),
                "supply-edge-collector.tar.gz",
                collectorLinuxHash,
                "linux",
                "x64"
            ),
            ["edge_processor_1.0.0_windows_x64"] = CreateArtifact(
                "edge_processor_1.0.0_windows_x64",
                Path.Combine("edge", "processor", "1.0.0", "windows-x64", "supply-edge-processor.zip"),
                "supply-edge-processor.zip",
                processorWindowsHash,
                "windows",
                "x64"
            ),
            ["edge_processor_1.0.0_linux_x64"] = CreateArtifact(
                "edge_processor_1.0.0_linux_x64",
                Path.Combine("edge", "processor", "1.0.0", "linux-x64", "supply-edge-processor.tar.gz"),
                "supply-edge-processor.tar.gz",
                processorLinuxHash,
                "linux",
                "x64"
            ),
        };

        var manifestRelease = new ManifestReleaseDocument()
        {
            Id = "edge_release_1.0.0",
            Channel = "stable",
            ReleaseVersion = "1.0.0",
            MinWizardVersion = "1.0.0",
            Components =
            [
                CreateComponent(
                    "Supply.Edge.Agent",
                    "Supply Edge Agent",
                    "1.0.0",
                    ["edge_agent_1.0.0_windows_x64", "edge_agent_1.0.0_linux_x64"],
                    []
                ),
                CreateComponent(
                    "Supply.Edge.Collector",
                    "Supply Edge Collector",
                    "1.0.0",
                    ["edge_collector_1.0.0_windows_x64", "edge_collector_1.0.0_linux_x64"],
                    []
                ),
                CreateComponent(
                    "Supply.Edge.Processor",
                    "Supply Edge Processor",
                    "1.0.0",
                    ["edge_processor_1.0.0_windows_x64", "edge_processor_1.0.0_linux_x64"],
                    ["redis"]
                ),
            ],
            Dependencies =
            [
                new ManifestDependencyDocument
                {
                    Id = "redis",
                    DisplayName = "Redis",
                    DefaultPolicy = DependencyPolicy.External,
                    ProbeScheme = "tcp",
                    ProbePort = 6379,
                },
            ],
        };

        var wizardRelease = new WizardBinaryReleaseDocument()
        {
            Id = "wizard_release_1.0.0",
            Channel = "stable",
            Version = "1.0.0",
            ArtifactIds = ["wizard_1.0.0_windows_x64", "wizard_1.0.0_linux_x64"],
        };

        return new CatalogDocument
        {
            Artifacts = artifacts,
            ManifestReleases = new Dictionary<string, ManifestReleaseDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [manifestRelease.Id] = manifestRelease,
            },
            WizardBinaryReleases = new Dictionary<string, WizardBinaryReleaseDocument>(StringComparer.OrdinalIgnoreCase)
            {
                [wizardRelease.Id] = wizardRelease,
            },
            ChannelPointers = new Dictionary<string, ChannelPointerDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable"] = new ChannelPointerDocument
                {
                    Channel = "stable",
                    ManifestReleaseId = manifestRelease.Id,
                    WizardBinaryReleaseId = wizardRelease.Id,
                },
            },
            CustomerPolicies = new Dictionary<string, CustomerPolicyDocument>(StringComparer.OrdinalIgnoreCase)
            {
                ["public"] = new CustomerPolicyDocument { AllowedChannels = ["stable"] },
            },
        };
    }

    private static ArtifactDocument CreateArtifact(
        string artifactId,
        string relativePath,
        string fileName,
        string sha256,
        string operatingSystem,
        string architecture
    )
    {
        return new ArtifactDocument
        {
            Id = artifactId,
            RelativePath = relativePath,
            FileName = fileName,
            ContentType = "application/octet-stream",
            SizeBytes = 1,
            Sha256 = sha256,
            PackageType = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "zip" : "tar.gz",
            Os = operatingSystem,
            Architecture = architecture,
            PublishedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    private static ManifestComponentDocument CreateComponent(
        string id,
        string displayName,
        string version,
        IReadOnlyList<string> artifactIds,
        IReadOnlyList<string> dependencyIds
    )
    {
        var lowerIdentifier = id.ToLowerInvariant().Replace(".", "-", StringComparison.Ordinal);
        var windowsProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var linuxInstallRootPath = Path.Combine(
            Path.DirectorySeparatorChar.ToString(),
            "opt",
            "supply",
            lowerIdentifier
        );
        return new ManifestComponentDocument
        {
            Id = id,
            DisplayName = displayName,
            Version = version,
            ArtifactIds = artifactIds,
            DependencyIds = dependencyIds,
            Service = new ServiceDocument
            {
                ServiceName = lowerIdentifier,
                DisplayName = displayName,
                ExecutablePath = id + ".exe",
                WorkingDirectoryPath = Path.Combine("%ProgramData%", "Supply", id),
                DefaultInstallPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["windows"] = Path.Combine(windowsProgramFilesPath, "Supply", id),
                    ["linux"] = linuxInstallRootPath,
                },
            },
        };
    }
}
