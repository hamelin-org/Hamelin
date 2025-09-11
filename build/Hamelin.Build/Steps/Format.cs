using System.ComponentModel;
using Hamelin.Build.Services;

namespace Hamelin.Build.Steps;

[DisplayName("Validate Code Formatting")]
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
