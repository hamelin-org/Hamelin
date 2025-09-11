using System.ComponentModel;
using Hamelin.Build.Services;

namespace Hamelin.Build.Steps;

[DisplayName("Restore Dependencies")]
public class Restore(ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["restore"],
            cancellationToken
        );
    }
}
