namespace Hamelin;

public class PipelineApplicationOptions
{
    public string[]? Args { get; init; }
    public string? EnvironmentName { get; init; }
    public string? ApplicationName { get; init; }
    public string? ContentRootPath { get; init; }
}
