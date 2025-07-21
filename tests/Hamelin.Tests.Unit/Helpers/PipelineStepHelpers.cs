namespace Hamelin.Tests.Unit.Helpers;

public static class PipelineStepHelpers
{
    public static IPipelineStep CreateMock()
    {
        var step = Substitute.For<IPipelineStep>();
        step.Run(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        return step;
    }
}
