namespace Hamelin;

/// <summary>
/// Represents a step in a CI/CD pipeline that can be executed.
/// </summary>
public interface IPipelineStep
{
    /// <summary>
    /// Runs the step in the pipeline.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when the step has finished running.</returns>
    Task Run(CancellationToken cancellationToken = default);
}
