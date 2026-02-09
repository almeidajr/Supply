using CliWrap;
using CliWrap.Buffered;
using Supply.Wizard.Application;
using Supply.Wizard.Application.Abstractions;

namespace Supply.Wizard.Infrastructure.System.Processes;

/// <summary>
/// Default process execution adapter.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>
    /// Executes a process and captures exit code, standard output, and standard error.
    /// </summary>
    /// <param name="spec">Process specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Buffered process result.</returns>
    public async Task<ProcessResult> RunAsync(ProcessSpec spec, CancellationToken cancellationToken)
    {
        var command = Cli.Wrap(spec.FileName)
            .WithArguments(argumentBuilder =>
            {
                foreach (var argument in spec.Arguments)
                {
                    argumentBuilder.Add(argument);
                }
            })
            .WithWorkingDirectory(spec.WorkingDirectoryPath ?? Directory.GetCurrentDirectory())
            .WithEnvironmentVariables(environmentBuilder =>
            {
                foreach (var (key, value) in spec.EnvironmentVariables)
                {
                    environmentBuilder.Set(key, value);
                }
            })
            .WithValidation(CommandResultValidation.None);

        var commandResult = await command.ExecuteBufferedAsync(cancellationToken);

        return new ProcessResult
        {
            ExitCode = commandResult.ExitCode,
            StandardOutput = commandResult.StandardOutput,
            StandardError = commandResult.StandardError,
        };
    }
}
