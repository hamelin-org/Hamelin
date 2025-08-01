using NuGet.Versioning;

namespace Hamelin.Build.Models;

public class ProjectInfo
{
    public required string Name { get; set; }
    public required NuGetVersion Version { get; init; }
}
