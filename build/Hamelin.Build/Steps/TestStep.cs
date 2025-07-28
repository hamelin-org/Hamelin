using Hamelin.Build.Services;

namespace Hamelin.Build.Steps;

public class TestStep(ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["test", "--no-build"],
            cancellationToken
        );
    }
}
