using Hamelin.Build.Services;
using Microsoft.Extensions.Options;

namespace Hamelin.Build.Steps;

public class Publish(IOptions<BuildOptions> options, IPipelineContext context, ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var packageFile = context.FileSystem.CurrentDirectory
            .GetDirectory(options.Value.ArtifactsDirectory)
            .GetFiles("*.nupkg")
            .Single();

        await commands.Run(
            command: "dotnet",
            arguments: [
                "nuget", "push",
                packageFile.AbsolutePath,
                "--source", options.Value.NuGetFeed,
                "--api-key", options.Value.NuGetApiKey
            ],
            cancellationToken
        );
    }
}
