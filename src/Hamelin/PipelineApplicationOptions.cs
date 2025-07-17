namespace Hamelin;

/// <summary>
/// Settings for constructing a <see cref="PipelineApplicationBuilder"/>.
/// </summary>
public class PipelineApplicationOptions
{
    /// <summary>
    /// Gets or sets the command-line arguments to add to the <see cref="PipelineApplicationBuilder.Configuration"/>.
    /// </summary>
    public string[]? Args { get; init; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string? EnvironmentName { get; init; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string? ApplicationName { get; init; }

    /// <summary>
    /// Gets or sets the content root path.
    /// </summary>
    public string? ContentRootPath { get; init; }
}
