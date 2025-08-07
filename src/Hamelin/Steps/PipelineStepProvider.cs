using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Steps;

internal class PipelineStepProvider(IPipelineStepCollection steps, IServiceProvider services) : IPipelineStepProvider
{
    public IEnumerable<IPipelineStep> GetSteps() => steps.GetSteps().Select(stepType => (IPipelineStep)services.GetRequiredService(stepType));
}
