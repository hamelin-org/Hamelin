using Hamelin.Build.Services;

namespace Hamelin.Build.Steps;

public class Format(ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["format", "--verify-no-changes"],
            cancellationToken
        );
    }
}
