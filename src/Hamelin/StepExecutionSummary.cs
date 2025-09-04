using System.Diagnostics.CodeAnalysis;

namespace Hamelin;

/// <summary>
/// Represents the summary of an executed pipeline step.
/// </summary>
public class StepExecutionSummary
{
    /// <summary>
    /// Creates a new <see cref="StepExecutionSummary"/>
    /// </summary>
    public StepExecutionSummary() { }

    /// <summary>
    /// Creates a new <see cref="StepExecutionSummary"/>
    /// </summary>
    /// <param name="stepName">The name of the step that this summary is for.</param>
    /// <param name="result">The result of executing the step.</param>
    [SetsRequiredMembers]
    public StepExecutionSummary(string stepName, PipelineStepResult result)
    {
        StepName = stepName;
        Result = result;
    }

    /// <summary>
    /// The name of the step that this summary is for.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// The result of executing the step.
    /// </summary>
    public required PipelineStepResult Result { get; init; }
}
