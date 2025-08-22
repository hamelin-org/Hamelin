
namespace Hamelin.Hooks;

/// <summary>
/// Allows for custom logic to be executed after each pipeline step.
/// </summary>
public interface IPostStepHook
{
    /// <summary>
    /// The method that will be called after each pipeline step.
    /// </summary>
    /// <param name="summary">The summary of the pipeline execution, including exit code and step results.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task PostStep(StepExecutionSummary summary, CancellationToken cancellationToken = default);
}
