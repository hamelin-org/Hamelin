namespace Hamelin;

/// <summary>
/// Describes the conditions under which the pipeline terminates early.
/// </summary>
public enum PipelineTerminationMode
{
    /// <summary>
    /// The pipeline should terminate on the first unhandled exception.
    /// </summary>
    StopOnUnhandledException,

    /// <summary>
    /// The pipeline should only terminate after all steps have been run.
    /// </summary>
    StopAfterAllSteps
}
