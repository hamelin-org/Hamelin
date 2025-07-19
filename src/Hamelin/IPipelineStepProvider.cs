namespace Hamelin;

/// <summary>
/// Represents a provider for pipeline steps.
/// </summary>
public interface IPipelineStepProvider
{
    /// <summary>
    /// Provides the collection of pipeline steps to run.
    /// </summary>
    /// <returns>The steps that should be run as part of the pipeline.</returns>
    IEnumerable<IPipelineStep> GetSteps();
}
