namespace Hamelin.Steps;

internal interface IPipelineStepCollection
{
    void AddStep<TStep>() where TStep : IPipelineStep;
    IReadOnlyCollection<Type> GetSteps();
}
