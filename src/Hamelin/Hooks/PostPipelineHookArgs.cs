namespace Hamelin.Hooks;

/// <summary>
/// Provides information about the pipeline that has just been run.
/// </summary>
public class PostPipelineHookArgs
{
    /// <summary>
    /// Gets the exit code of the overall pipeline execution.
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Gets the results of each step in the pipeline execution.
    /// </summary>
    public required IReadOnlyCollection<StepExecutionSummary> Steps { get; init; }
}
