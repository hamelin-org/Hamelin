using System.Text;
using CliWrap;
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
        var result = await command.ExecuteAsync(cancellationToken);

        if (result.ExitCode == 0)
        {
           logger.LogInformation("Output: {StdOut}", stdOutBuffer);
        }
        else
        {
            logger.LogError("Error: {StdErr}", stdErrBuffer);
            throw new Exception($"Command {command} returned exit code {result.ExitCode}");
        }
    }
}
