namespace Hamelin.Internal;

/// <summary>
/// Settings for the pipeline host
/// </summary>
public class PipelineHostOptions
{
    /// <summary>
    /// If `true` then application termination will be requested the pipeline run is completed
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public bool StopApplicationOnCompletion { get; init; } =  true;
}
