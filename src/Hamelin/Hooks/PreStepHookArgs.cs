namespace Hamelin.Hooks;

/// <summary>
/// Provides information about a pipeline step that is about to be run.
/// </summary>
public class PreStepHookArgs
{
    /// <summary>
    /// The name of the step.
    /// </summary>
    public required string StepName { get; init; }
}
