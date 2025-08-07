namespace Hamelin.Steps;

internal class PipelineStepCollection : IPipelineStepCollection
{
    private readonly List<Type> _steps = [];
    public void AddStep<TStep>() where TStep : IPipelineStep => _steps.Add(typeof(TStep));

    public IReadOnlyCollection<Type> GetSteps() => _steps;
}
