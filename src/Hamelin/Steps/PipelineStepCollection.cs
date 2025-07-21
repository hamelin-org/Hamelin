using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Steps;

internal class PipelineStepCollection : IPipelineStepCollector, IPipelineStepProvider
{
    private readonly List<Type> _steps = [];

    public void AddStep<TStep>() where TStep : class, IPipelineStep
    {
        _steps.Add(typeof(TStep));
    }

    public IEnumerable<IPipelineStep> GetSteps(IServiceProvider provider) => _steps.Select(stepType => (IPipelineStep)provider.GetRequiredService(stepType));
}
