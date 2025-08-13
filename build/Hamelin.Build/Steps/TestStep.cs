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

        string summary = """
                         ## Build Summary

                         Build completed successfully.
                         """;

        await File.WriteAllTextAsync(
            path: Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")!,
            contents: summary,
            cancellationToken: cancellationToken
        );
    }
}
