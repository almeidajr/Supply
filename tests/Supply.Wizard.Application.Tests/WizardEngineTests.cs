using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Application.Orchestration;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Tests;

public sealed class WizardEngineTests
{
    [Fact]
    public async Task ExecuteAsync_WhenRequestIsInvalid_ShouldReturnInvalidInput()
    {
        var manifestClient = new StubManifestClient(CreateManifest());
        var planBuilder = new StubPlanBuilder(
            (request, manifest, state, cancellationToken) => Task.FromResult(CreatePlan(request, manifest, state, []))
        );
        var planRunner = new StubPlanRunner(
            (plan, context, cancellationToken) =>
                Task.FromResult(new PlanExecutionResult { Succeeded = true, Message = "ok" })
        );
        var stateStore = new InMemoryStateStore();
        var engine = new WizardEngine(manifestClient, stateStore, planBuilder, planRunner);
        var request = CreateRequest() with { ApiBaseUri = new Uri("/relative", UriKind.Relative) };

        var result = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(WizardExitCode.InvalidInput, result.ExitCode);
        Assert.False(result.Succeeded);
        Assert.Equal(0, manifestClient.Calls);
        Assert.Equal(0, planBuilder.Calls);
        Assert.Equal(0, planRunner.Calls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlanIsEmpty_ShouldReturnSuccessWithoutRunningPlan()
    {
        var manifest = CreateManifest();
        var manifestClient = new StubManifestClient(manifest);
        var stateStore = new InMemoryStateStore();
        var plan = CreatePlan(CreateRequest(), manifest, new WizardState(), []);
        var planBuilder = new StubPlanBuilder(
            (request, manifestDocument, state, cancellationToken) => Task.FromResult(plan)
        );
        var planRunner = new StubPlanRunner(
            (executionPlan, context, cancellationToken) =>
                Task.FromResult(new PlanExecutionResult { Succeeded = true, Message = "completed" })
        );
        var engine = new WizardEngine(manifestClient, stateStore, planBuilder, planRunner);

        var result = await engine.ExecuteAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(WizardExitCode.Success, result.ExitCode);
        Assert.True(result.Succeeded);
        Assert.Equal("No changes are required.", result.Message);
        Assert.Same(plan, result.Plan);
        Assert.Null(result.RunResult);
        Assert.Equal(1, planBuilder.Calls);
        Assert.Equal(0, planRunner.Calls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlanRunnerSucceeds_ShouldReturnSuccessWithRunResult()
    {
        var request = CreateRequest();
        var manifest = CreateManifest();
        var state = new WizardState();
        var plan = CreatePlan(
            request,
            manifest,
            state,
            [new SpyPlanStep("step.one", isReversible: true, execute: static () => StepResult.Success("ok"))]
        );
        var expectedRunResult = new PlanExecutionResult
        {
            Succeeded = true,
            FinalState = state,
            Message = "done",
        };

        var engine = new WizardEngine(
            new StubManifestClient(manifest),
            new InMemoryStateStore(),
            new StubPlanBuilder((_, _, _, _) => Task.FromResult(plan)),
            new StubPlanRunner((_, _, _) => Task.FromResult(expectedRunResult))
        );

        var result = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(WizardExitCode.Success, result.ExitCode);
        Assert.True(result.Succeeded);
        Assert.Equal("done", result.Message);
        Assert.Same(plan, result.Plan);
        Assert.Same(expectedRunResult, result.RunResult);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunnerFailsAndRollbackSucceeds_ShouldReturnRollbackSucceededExitCode()
    {
        var request = CreateRequest();
        var manifest = CreateManifest();
        var plan = CreatePlan(
            request,
            manifest,
            new WizardState(),
            [new SpyPlanStep("step.one", isReversible: true, execute: static () => StepResult.Success("ok"))]
        );

        var engine = new WizardEngine(
            new StubManifestClient(manifest),
            new InMemoryStateStore(),
            new StubPlanBuilder((_, _, _, _) => Task.FromResult(plan)),
            new StubPlanRunner(
                (_, _, _) =>
                    Task.FromResult(
                        new PlanExecutionResult
                        {
                            Succeeded = false,
                            RollbackAttempted = true,
                            RollbackSucceeded = true,
                            Message = "failed",
                        }
                    )
            )
        );

        var result = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(WizardExitCode.ExecutionFailureRollbackSucceeded, result.ExitCode);
        Assert.False(result.Succeeded);
        Assert.Equal("failed", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunnerFailsAndRollbackFails_ShouldReturnRollbackFailedExitCode()
    {
        var request = CreateRequest();
        var manifest = CreateManifest();
        var plan = CreatePlan(
            request,
            manifest,
            new WizardState(),
            [new SpyPlanStep("step.one", isReversible: true, execute: static () => StepResult.Success("ok"))]
        );

        var engine = new WizardEngine(
            new StubManifestClient(manifest),
            new InMemoryStateStore(),
            new StubPlanBuilder((_, _, _, _) => Task.FromResult(plan)),
            new StubPlanRunner(
                (_, _, _) =>
                    Task.FromResult(
                        new PlanExecutionResult
                        {
                            Succeeded = false,
                            RollbackAttempted = true,
                            RollbackSucceeded = false,
                            Message = "failed",
                        }
                    )
            )
        );

        var result = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(WizardExitCode.ExecutionFailureRollbackFailed, result.ExitCode);
        Assert.False(result.Succeeded);
        Assert.Equal("failed", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasChannelAndSecurityOptions_ShouldForwardManifestQuery()
    {
        var manifest = CreateManifest();
        var request = CreateRequest() with
        {
            Channel = "preview",
            Authentication = new WizardAuthOptions
            {
                BearerToken = "token",
                ClientCertificateFilePath = "cert.pem",
                ClientCertificateKeyFilePath = "key.pem",
            },
            Tls = new WizardTlsOptions
            {
                AllowInsecureServerCertificate = true,
                CustomCaCertificateFilePath = "ca.pem",
            },
        };
        var manifestClient = new StubManifestClient(manifest);
        var engine = new WizardEngine(
            manifestClient,
            new InMemoryStateStore(),
            new StubPlanBuilder(
                (wizardRequest, manifestDocument, state, cancellationToken) =>
                    Task.FromResult(CreatePlan(wizardRequest, manifestDocument, state, []))
            ),
            new StubPlanRunner((_, _, _) => Task.FromResult(new PlanExecutionResult { Succeeded = true }))
        );

        _ = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.NotNull(manifestClient.LastQuery);
        Assert.Equal(request.ApiBaseUri, manifestClient.LastQuery.ApiBaseUri);
        Assert.Equal("preview", manifestClient.LastQuery.Channel);
        Assert.Equal("token", manifestClient.LastQuery.Authentication.BearerToken);
        Assert.Equal("cert.pem", manifestClient.LastQuery.Authentication.ClientCertificateFilePath);
        Assert.Equal("key.pem", manifestClient.LastQuery.Authentication.ClientCertificateKeyFilePath);
        Assert.True(manifestClient.LastQuery.Tls.AllowInsecureServerCertificate);
        Assert.Equal("ca.pem", manifestClient.LastQuery.Tls.CustomCaCertificateFilePath);
    }

    [Theory]
    [MemberData(nameof(ExecuteAsyncExceptionCases))]
    public async Task ExecuteAsync_WhenDependencyThrows_ShouldMapExceptionToExitCode(
        string failureKind,
        WizardExitCode expectedExitCode
    )
    {
        var exception = CreateException(failureKind);
        var request = CreateRequest();
        var manifest = CreateManifest();
        var engine = new WizardEngine(
            new StubManifestClient(manifest),
            new InMemoryStateStore(),
            new StubPlanBuilder((_, _, _, _) => Task.FromException<ExecutionPlan>(exception)),
            new StubPlanRunner((_, _, _) => Task.FromResult(new PlanExecutionResult { Succeeded = true }))
        );

        var result = await engine.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.False(result.Succeeded);
        Assert.Equal(exception.Message, result.Message);
    }

    public static TheoryData<string, WizardExitCode> ExecuteAsyncExceptionCases =>
        new()
        {
            { "validation", WizardExitCode.InvalidInput },
            { "dependency", WizardExitCode.DependencyValidationFailure },
            { "artifact", WizardExitCode.ManifestOrArtifactFailure },
            { "api", WizardExitCode.ApiOrAuthenticationFailure },
            { "unexpected", WizardExitCode.UnexpectedFailure },
        };

    private static WizardRequest CreateRequest()
    {
        return new WizardRequest
        {
            Operation = OperationKind.Install,
            ApiBaseUri = new Uri("https://localhost:5001"),
            CacheDirectoryPath = Path.Combine(Path.GetTempPath(), "supply-tests", "cache"),
            StateFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "state.json"),
            JournalFilePath = Path.Combine(Path.GetTempPath(), "supply-tests", "journal.jsonl"),
        };
    }

    private static ManifestDocument CreateManifest()
    {
        return new ManifestDocument { ManifestVersion = "test", PublishedAtUtc = DateTimeOffset.UtcNow };
    }

    private static ExecutionPlan CreatePlan(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state,
        IReadOnlyList<IPlanStep> steps
    )
    {
        return new ExecutionPlan
        {
            RunId = Guid.NewGuid(),
            Operation = request.Operation,
            Request = request,
            Manifest = manifest,
            InitialState = state,
            TargetState = state with { },
            Steps = steps,
        };
    }

    private static Exception CreateException(string failureKind)
    {
        return failureKind switch
        {
            "validation" => new WizardValidationException("validation"),
            "dependency" => new DependencyValidationException("dependency"),
            "artifact" => new ArtifactIntegrityException("artifact"),
            "api" => new ApiAccessException("api"),
            _ => new InvalidOperationException("unexpected"),
        };
    }

    private sealed class StubManifestClient(Func<ManifestQuery, CancellationToken, Task<ManifestDocument>> handler)
        : IManifestClient
    {
        public StubManifestClient(ManifestDocument manifest)
            : this((_, _) => Task.FromResult(manifest)) { }

        public int Calls { get; private set; }

        public ManifestQuery? LastQuery { get; private set; }

        public Task<ManifestDocument> GetManifestAsync(ManifestQuery query, CancellationToken cancellationToken)
        {
            Calls++;
            LastQuery = query;
            return handler(query, cancellationToken);
        }
    }

    private sealed class StubPlanBuilder(
        Func<WizardRequest, ManifestDocument, WizardState, CancellationToken, Task<ExecutionPlan>> handler
    ) : IPlanBuilder
    {
        public int Calls { get; private set; }

        public Task<ExecutionPlan> BuildAsync(
            WizardRequest request,
            ManifestDocument manifest,
            WizardState state,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return handler(request, manifest, state, cancellationToken);
        }
    }

    private sealed class StubPlanRunner(
        Func<ExecutionPlan, RunContext, CancellationToken, Task<PlanExecutionResult>> handler
    ) : IPlanRunner
    {
        public int Calls { get; private set; }

        public Task<PlanExecutionResult> RunAsync(
            ExecutionPlan plan,
            RunContext context,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return handler(plan, context, cancellationToken);
        }
    }
}
