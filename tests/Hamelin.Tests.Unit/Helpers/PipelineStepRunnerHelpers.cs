using Hamelin.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Tests.Unit.Helpers;

internal static class PipelineStepRunnerHelpers
{
    public static IPipelineStepRunner CreateMock()
    {
        var stepRunner = Substitute.For<IPipelineStepRunner>();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), Arg.Any<IPipelineStep>(), Arg.Any<CancellationToken>())
            .Returns(PipelineStepResult.Successful);
        return stepRunner;
    }
}
