namespace Hamelin.Hooks;

/// <summary>
/// Provides information about a pipeline step that has just been run.
/// </summary>
public class PostStepHookArgs
{
    /// <summary>
    /// The name of the step.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// The result of executing the step.
    /// </summary>
    public required PipelineStepResult Result { get; init; }
}
