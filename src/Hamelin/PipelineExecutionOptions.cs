namespace Hamelin;

/// <summary>
/// Settings to control the execution of the pipeline
/// </summary>
public class PipelineExecutionOptions
{
    /// <summary>
    /// If `true` then application termination will be requested when the pipeline run is completed.
    /// </summary>
    public bool StopApplicationOnCompletion { get; set; } = true;

    /// <summary>
    /// Controls what causes the pipeline to terminate early.
    /// </summary>
    public PipelineTerminationMode TerminationMode { get; set; } = PipelineTerminationMode.StopOnUnhandledException;

    /// <summary>
    /// If `true` then the pipeline will set the exit code automatically on failure.
    /// </summary>
    public bool EnableAutomaticExitCodes { get; set; } = true;
}
