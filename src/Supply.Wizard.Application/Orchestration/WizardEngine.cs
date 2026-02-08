using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Orchestration;

/// <summary>
/// Coordinates manifest retrieval, planning, and execution.
/// </summary>
public sealed class WizardEngine(
    IManifestClient manifestClient,
    IStateStore stateStore,
    IPlanBuilder planBuilder,
    IPlanRunner planRunner
) : IWizardEngine
{
    /// <inheritdoc />
    public async Task<WizardExecutionResult> ExecuteAsync(WizardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            ValidateRequest(request);

            var state = await stateStore.LoadAsync(request.StateFilePath, cancellationToken);
            var manifest = await manifestClient.GetManifestAsync(
                new ManifestQuery
                {
                    ApiBaseUri = request.ApiBaseUri,
                    Channel = request.Channel,
                    Authentication = request.Authentication,
                    Tls = request.Tls,
                },
                cancellationToken
            );

            var plan = await planBuilder.BuildAsync(request, manifest, state, cancellationToken);
            if (plan.IsEmpty)
            {
                return new WizardExecutionResult
                {
                    ExitCode = WizardExitCode.Success,
                    Succeeded = true,
                    DryRun = request.DryRun,
                    Message = "No changes are required.",
                    Plan = plan,
                };
            }

            var planExecutionResult = await planRunner.RunAsync(
                plan,
                new RunContext { Request = request, DryRun = request.DryRun },
                cancellationToken
            );

            if (planExecutionResult.Succeeded)
            {
                return new WizardExecutionResult
                {
                    ExitCode = WizardExitCode.Success,
                    Succeeded = true,
                    DryRun = request.DryRun,
                    Message = planExecutionResult.Message,
                    Plan = plan,
                    RunResult = planExecutionResult,
                };
            }

            return new WizardExecutionResult
            {
                ExitCode =
                    planExecutionResult.RollbackAttempted && planExecutionResult.RollbackSucceeded
                        ? WizardExitCode.ExecutionFailureRollbackSucceeded
                        : WizardExitCode.ExecutionFailureRollbackFailed,
                Succeeded = false,
                DryRun = request.DryRun,
                Message = planExecutionResult.Message,
                Plan = plan,
                RunResult = planExecutionResult,
            };
        }
        catch (WizardValidationException exception)
        {
            return Failure(WizardExitCode.InvalidInput, exception.Message, request.DryRun);
        }
        catch (DependencyValidationException exception)
        {
            return Failure(WizardExitCode.DependencyValidationFailure, exception.Message, request.DryRun);
        }
        catch (ArtifactIntegrityException exception)
        {
            return Failure(WizardExitCode.ManifestOrArtifactFailure, exception.Message, request.DryRun);
        }
        catch (ApiAccessException exception)
        {
            return Failure(WizardExitCode.ApiOrAuthenticationFailure, exception.Message, request.DryRun);
        }
        catch (Exception exception)
        {
            return Failure(WizardExitCode.UnexpectedFailure, exception.Message, request.DryRun);
        }
    }

    private static WizardExecutionResult Failure(WizardExitCode exitCode, string message, bool dryRun)
    {
        return new WizardExecutionResult
        {
            ExitCode = exitCode,
            Succeeded = false,
            DryRun = dryRun,
            Message = message,
        };
    }

    private static void ValidateRequest(WizardRequest request)
    {
        if (!request.ApiBaseUri.IsAbsoluteUri)
        {
            throw new WizardValidationException("API base URI must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(request.CacheDirectoryPath))
        {
            throw new WizardValidationException("Cache directory path must be provided.");
        }

        if (string.IsNullOrWhiteSpace(request.StateFilePath))
        {
            throw new WizardValidationException("State file path must be provided.");
        }

        if (string.IsNullOrWhiteSpace(request.JournalFilePath))
        {
            throw new WizardValidationException("Journal file path must be provided.");
        }
    }
}
