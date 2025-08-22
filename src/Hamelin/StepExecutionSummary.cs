namespace Hamelin;

/// <summary>
/// Represents the summary of an executed pipeline step.
/// </summary>
public class StepExecutionSummary
{
    /// <summary>
    /// The name of the step that this summary is for.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// The result of executing the step.
    /// </summary>
    public required PipelineStepResult Result { get; init; }
}
