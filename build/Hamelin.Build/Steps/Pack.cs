using Hamelin.Build.Services;
using Microsoft.Extensions.Options;

namespace Hamelin.Build.Steps;

public class Pack(IOptions<BuildOptions> options, ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: [
                "pack",
                "--no-build",
                "--configuration", options.Value.Configuration,
                "--output", options.Value.ArtifactsDirectory
            ],
            cancellationToken
        );
    }
}
