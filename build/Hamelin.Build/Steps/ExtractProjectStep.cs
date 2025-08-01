using System.Xml;
using Hamelin.Build.Models;
using Microsoft.Extensions.Options;
using NuGet.Versioning;

namespace Hamelin.Build.Steps;

public class ExtractProjectStep(IOptions<BuildOptions> options, IPipelineContext context) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var csproj = context.FileSystem.CurrentDirectory.GetFile(options.Value.ProjectFile);
        if (!csproj.Exists)
        {
            throw new FileNotFoundException("Could not find project file", csproj.AbsolutePath);
        }

        await using var projectStream = csproj.OpenRead();
        var report = new XmlDocument();
        report.Load(projectStream);
        if (report.DocumentElement == null)
        {
            throw new Exception("Could not parse csproj");
        }

        string? packageName = report.SelectSingleNode("Project/PropertyGroup/PackageId")?.FirstChild?.Value;
        if (packageName == null)
        {
            throw new Exception("Unable to find PackageId in project file.");
        }

        string? packageVersion = report.SelectSingleNode("Project/PropertyGroup/Version")?.FirstChild?.Value;
        if (packageVersion == null)
        {
            throw new Exception("Unable to find Version in project file.");
        }

        var projectInfo = new ProjectInfo()
        {
            Name = packageName,
            Version = NuGetVersion.Parse(packageVersion)
        };
        context.State.Set(projectInfo);
    }
}
