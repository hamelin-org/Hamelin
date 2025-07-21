namespace Hamelin.Steps;

/// <summary>
/// Represents a provider for pipeline steps.
/// </summary>
public interface IPipelineStepProvider
{
    /// <summary>
    /// Provides the collection of pipeline steps to run.
    /// </summary>
    /// <param name="provider">The service provider from which to retrieve the steps, if required.</param>
    /// <returns>The steps that should be run as part of the pipeline.</returns>
    IEnumerable<IPipelineStep> GetSteps(IServiceProvider provider);
}
