namespace Hamelin.Steps;

internal class PipelineStepCollection : IPipelineStepCollection
{
    private readonly List<Type> _steps = [];

    public void AddStep(Type step)
    {
        if (!step.IsAssignableTo(typeof(IPipelineStep)))
        {
            throw new ArgumentException($"The type {step.FullName} does not implement {nameof(IPipelineStep)} and cannot be added as a pipeline step.");
        }
        _steps.Add(step);
    }

    public IReadOnlyCollection<Type> GetSteps() => _steps;
}
