using System.Text;
using CliWrap;
using CliWrap.EventStream;
using Microsoft.Extensions.Logging;

namespace Hamelin.Build.Steps;

public class FormatStep(ILogger<FormatStep> logger, IPipelineContext context) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var command = Cli.Wrap("dotnet")
            .WithArguments(["format", "--verify-no-changes"])
            .WithWorkingDirectory(context.CurrentDirectory)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None);

        logger.LogInformation("Running command: {Command}", command);

        await foreach (var cmdEvent in command.ListenAsync(cancellationToken))
        {
            switch (cmdEvent)
            {
                case StartedCommandEvent started:
                    logger.LogInformation("Process started; ID: {ProcessId}", started.ProcessId);
                    break;
                case StandardOutputCommandEvent stdOut:
                    logger.LogInformation("{Output}", stdOut.Text);
                    break;
                case StandardErrorCommandEvent stdErr:
                    logger.LogError("{Error}", stdErr.Text);
                    break;
                case ExitedCommandEvent exited:
                    logger.LogInformation("Process exited; Code: {ExitCode}", exited.ExitCode);
                    if (exited.ExitCode != 0)
                    {
                        throw new Exception($"Command {command} returned exit code {exited.ExitCode}");
                    }
                    break;
            }
        }
    }
}
