using Hamelin.Build.Helpers;
using Hamelin.Build.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Hamelin.Build.Steps;

public class VersionStep(
    ILoggerFactory loggerFactory,
    IOptions<BuildOptions> options,
    IPipelineContext context
) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var projectInfo = context.State.Get<ProjectInfo>();
        if (projectInfo == null)
        {
            throw new Exception("Project info not found in state.");
        }

        PackageSourceCredential credentials = new(
            options.Value.NuGetFeed,
            "dummy",
            "",
            true,
            null
        );

        var packageSource = new PackageSource(options.Value.NuGetFeed) { Credentials = credentials };
        SourceRepository repository = Repository.Factory.GetCoreV3(packageSource);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            projectInfo.Name,
            new SourceCacheContext(),
            new NuGetLoggerAdapter(loggerFactory.CreateLogger<FindPackageByIdResource>()),
            cancellationToken
        );

        NuGetVersion? match = versions.FirstOrDefault(c => c == projectInfo.Version);
        if (match != null)
        {
            throw new Exception("Package version already exists.");
        }
    }
}
