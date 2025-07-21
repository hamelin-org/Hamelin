namespace Hamelin;

/// <summary>
/// Settings to control the execution of the pipeline
/// </summary>
public class PipelineExecutionOptions
{
    /// <summary>
    /// If `true` then application termination will be requested the pipeline run is completed
    /// </summary>
    public bool StopApplicationOnCompletion { get; init; } =  true;
}
