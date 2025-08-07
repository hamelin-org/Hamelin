using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Steps;

internal class PipelineStepCollection : List<Type>;

internal class PipelineStepProvider(PipelineStepCollection steps, IServiceProvider services) : IPipelineStepProvider
{
    public IEnumerable<IPipelineStep> GetSteps() => steps.Select(stepType => (IPipelineStep)services.GetRequiredService(stepType));
}
