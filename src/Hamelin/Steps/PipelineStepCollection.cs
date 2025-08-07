namespace Hamelin.Steps;

internal class PipelineStepCollection : List<Type>, IPipelineStepCollector
{
    public void AddStep<TStep>() where TStep : class, IPipelineStep => Add(typeof(TStep));
}
