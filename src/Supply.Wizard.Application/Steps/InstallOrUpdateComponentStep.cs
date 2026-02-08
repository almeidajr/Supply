using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Steps;

/// <summary>
/// Downloads, verifies, and activates a component as a platform service.
/// </summary>
public sealed class InstallOrUpdateComponentStep(ComponentManifest component, ArtifactManifest artifact) : IPlanStep
{
    private string? _installedPath;
    private string? _serviceName;

    /// <inheritdoc />
    public string Id => $"component.apply.{component.Id}";

    /// <inheritdoc />
    public string Name => $"Install or update '{component.Id}'";

    /// <inheritdoc />
    public bool IsReversible => true;

    /// <inheritdoc />
    public async Task<StepResult> ExecuteAsync(StepContext context, CancellationToken cancellationToken)
    {
        if (context.Request.DryRun)
        {
            return StepResult.Success($"Dry-run: skipped install/update for {component.Id}.");
        }

        var downloadResult = await context.ArtifactDownloader.DownloadAsync(
            artifact,
            new DownloadContext
            {
                CacheDirectoryPath = context.Request.CacheDirectoryPath,
                Authentication = context.Request.Authentication,
                Tls = context.Request.Tls,
            },
            cancellationToken
        );

        await context.ChecksumVerifier.VerifySha256Async(downloadResult.FilePath, artifact.Sha256, cancellationToken);

        var stateDirectoryPath =
            Path.GetDirectoryName(context.Request.StateFilePath) ?? Directory.GetCurrentDirectory();
        _installedPath = Path.Combine(stateDirectoryPath, "components", component.Id, component.Version);
        Directory.CreateDirectory(_installedPath);

        var artifactDestinationPath = Path.Combine(_installedPath, artifact.FileName);
        File.Copy(downloadResult.FilePath, artifactDestinationPath, overwrite: true);

        var manifestExecutablePath = component.Service.ExecutablePath;
        var executablePath = Path.IsPathRooted(manifestExecutablePath)
            ? manifestExecutablePath
            : Path.Combine(_installedPath, manifestExecutablePath);

        _serviceName = string.IsNullOrWhiteSpace(component.Service.ServiceName)
            ? component.Id
            : component.Service.ServiceName;
        var serviceDefinition = component.Service with
        {
            ServiceName = _serviceName,
            DisplayName = string.IsNullOrWhiteSpace(component.Service.DisplayName)
                ? component.DisplayName
                : component.Service.DisplayName,
            ExecutablePath = executablePath,
            WorkingDirectoryPath = string.IsNullOrWhiteSpace(component.Service.WorkingDirectoryPath)
                ? _installedPath
                : component.Service.WorkingDirectoryPath,
        };

        await context.ServiceManager.CreateOrUpdateAsync(serviceDefinition, cancellationToken);
        await context.ServiceManager.StartAsync(_serviceName, cancellationToken);

        context.State.Components[component.Id] = new InstalledComponentState
        {
            ComponentId = component.Id,
            Version = component.Version,
            InstalledPath = _installedPath,
            ServiceName = _serviceName,
            InstalledAtUtc = DateTimeOffset.UtcNow,
        };

        return StepResult.Success($"Component '{component.Id}' is installed at '{_installedPath}'.");
    }

    /// <inheritdoc />
    public async Task RollbackAsync(StepContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_serviceName) || string.IsNullOrWhiteSpace(_installedPath))
        {
            return;
        }

        try
        {
            await context.ServiceManager.StopAsync(_serviceName, cancellationToken);
        }
        catch
        {
            // Best effort rollback.
        }

        try
        {
            await context.ServiceManager.DeleteAsync(_serviceName, cancellationToken);
        }
        catch
        {
            // Best effort rollback.
        }

        try
        {
            if (Directory.Exists(_installedPath))
            {
                Directory.Delete(_installedPath, recursive: true);
            }
        }
        catch
        {
            // Best effort rollback.
        }

        context.State.Components.Remove(component.Id);
    }
}
