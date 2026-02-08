using System.Runtime.InteropServices;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Application.Planning;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Tests;

public sealed class DefaultPlanBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithManagedDependency_ShouldOrderDependencyBeforeComponent()
    {
        var planBuilder = new DefaultPlanBuilder();
        var manifest = CreateManifest(
            [
                CreateComponent("redis", "1.0.0", dependencyIds: []),
                CreateComponent("collector", "1.0.0", dependencyIds: ["redis-core"]),
            ],
            [
                new DependencyManifest
                {
                    Id = "redis-core",
                    DisplayName = "Redis",
                    DefaultPolicy = DependencyPolicy.Managed,
                    ManagedComponentId = "redis",
                },
            ]
        );

        var request = CreateRequest(OperationKind.Install, ["collector"]);
        var plan = await planBuilder.BuildAsync(request, manifest, new WizardState(), CancellationToken.None);

        Assert.Collection(
            plan.Steps,
            step => Assert.Equal("component.apply.redis", step.Id),
            step => Assert.Equal("component.apply.collector", step.Id)
        );
    }

    [Fact]
    public async Task BuildAsync_WhenDependencyIsExternalWithoutEndpoint_ShouldThrowDependencyValidationException()
    {
        var planBuilder = new DefaultPlanBuilder();
        var manifest = CreateManifest(
            [CreateComponent("collector", "1.0.0", dependencyIds: ["redis-core"])],
            [
                new DependencyManifest
                {
                    Id = "redis-core",
                    DisplayName = "Redis",
                    DefaultPolicy = DependencyPolicy.External,
                },
            ]
        );

        var request = CreateRequest(OperationKind.Install, ["collector"]);

        await Assert.ThrowsAsync<DependencyValidationException>(() =>
            planBuilder.BuildAsync(request, manifest, new WizardState(), CancellationToken.None)
        );
    }

    [Fact]
    public async Task BuildAsync_Update_WhenVersionsAreEqual_ShouldSkipUnchangedComponents()
    {
        var planBuilder = new DefaultPlanBuilder();
        var manifest = CreateManifest(
            [
                CreateComponent("agent", "2.0.0", dependencyIds: []),
                CreateComponent("collector", "2.0.0", dependencyIds: []),
            ],
            []
        );

        var state = new WizardState
        {
            Components = new Dictionary<string, InstalledComponentState>(StringComparer.OrdinalIgnoreCase)
            {
                ["agent"] = new InstalledComponentState
                {
                    ComponentId = "agent",
                    Version = "2.0.0",
                    ServiceName = "agent",
                    InstalledPath = "c:\\supply\\agent",
                },
                ["collector"] = new InstalledComponentState
                {
                    ComponentId = "collector",
                    Version = "1.0.0",
                    ServiceName = "collector",
                    InstalledPath = "c:\\supply\\collector",
                },
            },
        };

        var request = CreateRequest(OperationKind.Update, []);
        var plan = await planBuilder.BuildAsync(request, manifest, state, CancellationToken.None);

        Assert.Single(plan.Steps);
        Assert.Equal("component.apply.collector", plan.Steps[0].Id);
    }

    [Fact]
    public async Task BuildAsync_WhenOperationIsUninstall_ShouldCreateUninstallStepsInReverseDependencyOrder()
    {
        var planBuilder = new DefaultPlanBuilder();
        var manifest = CreateManifest(
            [
                CreateComponent("database", "1.0.0", dependencyIds: []),
                CreateComponent("processor", "1.0.0", dependencyIds: [], dependsOnComponentIds: ["database"]),
            ],
            []
        );
        var state = new WizardState
        {
            Components = new Dictionary<string, InstalledComponentState>(StringComparer.OrdinalIgnoreCase)
            {
                ["database"] = new InstalledComponentState { ComponentId = "database", Version = "1.0.0" },
                ["processor"] = new InstalledComponentState { ComponentId = "processor", Version = "1.0.0" },
            },
        };

        var request = CreateRequest(OperationKind.Uninstall, []);
        var plan = await planBuilder.BuildAsync(request, manifest, state, CancellationToken.None);

        Assert.Collection(
            plan.Steps,
            step => Assert.Equal("component.remove.processor", step.Id),
            step => Assert.Equal("component.remove.database", step.Id)
        );
        Assert.Empty(plan.TargetState.Components);
    }

    [Fact]
    public async Task BuildAsync_WhenComponentGraphHasCycle_ShouldThrowWizardValidationException()
    {
        var planBuilder = new DefaultPlanBuilder();
        var manifest = CreateManifest(
            [
                CreateComponent("a", "1.0.0", dependencyIds: [], dependsOnComponentIds: ["b"]),
                CreateComponent("b", "1.0.0", dependencyIds: [], dependsOnComponentIds: ["a"]),
            ],
            []
        );

        var request = CreateRequest(OperationKind.Install, ["a", "b"]);

        await Assert.ThrowsAsync<WizardValidationException>(() =>
            planBuilder.BuildAsync(request, manifest, new WizardState(), CancellationToken.None)
        );
    }

    private static WizardRequest CreateRequest(OperationKind operation, IReadOnlyList<string> targetComponents)
    {
        return new WizardRequest
        {
            Operation = operation,
            ApiBaseUri = new Uri("https://localhost:5001"),
            CacheDirectoryPath = Path.Combine(Path.GetTempPath(), "supply-tests", "cache"),
            StateFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "state.json"),
            JournalFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "journal.jsonl"),
            TargetComponentIds = targetComponents,
        };
    }

    private static ManifestDocument CreateManifest(
        IReadOnlyList<ComponentManifest> components,
        IReadOnlyList<DependencyManifest> dependencies
    )
    {
        return new ManifestDocument
        {
            ManifestVersion = "test",
            PublishedAtUtc = DateTimeOffset.UtcNow,
            Components = components,
            Dependencies = dependencies,
        };
    }

    private static ComponentManifest CreateComponent(
        string id,
        string version,
        IReadOnlyList<string> dependencyIds,
        IReadOnlyList<string>? dependsOnComponentIds = null
    )
    {
        return new ComponentManifest
        {
            Id = id,
            DisplayName = id,
            Version = version,
            EnabledByDefault = true,
            DependsOnComponentIds = dependsOnComponentIds ?? [],
            DependencyIds = dependencyIds,
            Artifacts = [CreateArtifact($"{id}.zip")],
            Service = new ServiceDefinition
            {
                ServiceName = id,
                DisplayName = id,
                ExecutablePath = $"{id}.exe",
            },
        };
    }

    private static ArtifactManifest CreateArtifact(string fileName)
    {
        var os = OperatingSystem.IsWindows() ? "windows" : "linux";
        var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        return new ArtifactManifest
        {
            Os = os,
            Architecture = architecture,
            DownloadUri = new Uri($"https://example.invalid/{fileName}"),
            FileName = fileName,
            Sha256 = "abc123",
        };
    }
}
