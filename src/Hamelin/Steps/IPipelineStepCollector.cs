namespace Hamelin.Steps;

internal interface IPipelineStepCollector
{
    void AddStep<TStep>() where TStep : class, IPipelineStep;
}
