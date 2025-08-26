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

    /// <summary>
    /// Creates an execution summary for the given step.
    /// </summary>
    /// <param name="step">The step to create the summary for.</param>
    /// <param name="result">The result of the execution.</param>
    /// <returns>The created summary.</returns>
    public static StepExecutionSummary FromStep(IPipelineStep step, PipelineStepResult result)
    {
        var stepType = step.GetType();
        string stepName = stepType.Name;

        return new StepExecutionSummary
        {
            StepName = stepName,
            Result = result
        };
    }
}
