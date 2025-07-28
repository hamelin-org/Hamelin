using Hamelin.Build.Services;
using Microsoft.Extensions.Options;

namespace Hamelin.Build.Steps;

public class BuildStep(IOptions<BuildOptions> options, ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["build", "--no-restore", "--configuration", options.Value.Configuration],
            cancellationToken
        );
    }
}
