using Microsoft.Extensions.DependencyInjection;

namespace Hamelin;

internal class PipelineStepCollection(IServiceProvider services)
{
    private readonly List<Type> _steps = [];

    public void AddStep<TStep>() where TStep : class, IPipelineStep
    {
        _steps.Add(typeof(TStep));
    }

    public IEnumerable<IPipelineStep> GetSteps() => _steps.Select(stepType => (IPipelineStep)services.GetRequiredService(stepType));
}
