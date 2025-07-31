namespace Hamelin.Build;

public class BuildOptions
{
    public required string ArtifactsDirectory { get; set; }
    public required string TempDirectory { get; set; }
    public required string Configuration { get; set; }
    public required string ProjectFile { get; set; }
    public required string NuGetFeed { get; set; }
}
