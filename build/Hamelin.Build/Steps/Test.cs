using System.ComponentModel;
using Hamelin.Build.Services;
using Microsoft.Extensions.Options;

namespace Hamelin.Build.Steps;

[DisplayName("Run Tests")]
public class Test(IOptions<BuildOptions> options, ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        await commands.Run(
            command: "dotnet",
            arguments: ["test", "--no-build", "--configuration", options.Value.Configuration],
            cancellationToken
        );
    }
}
