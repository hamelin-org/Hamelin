using Hamelin.Build.Services;
using Hamelin.Runtimes.GitHubActions;
using Microsoft.Extensions.Options;

namespace Hamelin.Build.Steps;

public class TestStep(
    IOptions<BuildOptions> options,
    ICommandRunner commands,
    IGitHubActionsCommands gha
) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["test", "--no-build", "--configuration", options.Value.Configuration],
            cancellationToken
        );

        gha.SetJobSummary(
            """
            ## Build Summary

            Build completed successfully.
            """
        );
    }
}
