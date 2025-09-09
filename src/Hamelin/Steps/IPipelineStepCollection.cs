namespace Hamelin.Steps;

internal interface IPipelineStepCollection
{
    void AddStep(Type step);
    IReadOnlyCollection<Type> GetSteps();
}
