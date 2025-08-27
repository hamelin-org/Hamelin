using Hamelin.Hooks;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Tests.Unit.Internal;

public class DefaultPipelineStepRunnerTests
{
    [Fact]
    public async Task RunStep_RunsStep()
    {
        // Arrange
        var step = Substitute.For<IPipelineStep>();
        var sut = DefaultPipelineStepRunnerHelpers.CreateRunner();

        var scope = new ServiceCollection()
            .BuildServiceProvider()
            .CreateAsyncScope();

        // Act
        await sut.RunStep(scope, step, CancellationToken.None);

        // Assert
        await step.Received().Run(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunStep_WithPreStepHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPreStepHook>();
        var hook2 = Substitute.For<IPreStepHook>();
        var step = Substitute.For<IPipelineStep>();
        var sut = DefaultPipelineStepRunnerHelpers.CreateRunner();

        var scope = new ServiceCollection()
            .AddSingleton(hook1)
            .AddSingleton(hook2)
            .BuildServiceProvider()
            .CreateAsyncScope();

        // Act
        await sut.RunStep(scope, step, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            hook1.PreStep(Arg.Any<CancellationToken>());
            hook2.PreStep(Arg.Any<CancellationToken>());
            step.Run(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunStep_WithPostStepHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPostStepHook>();
        var hook2 = Substitute.For<IPostStepHook>();
        var step = Substitute.For<IPipelineStep>();
        var sut = DefaultPipelineStepRunnerHelpers.CreateRunner();

        var scope = new ServiceCollection()
            .AddSingleton(hook1)
            .AddSingleton(hook2)
            .BuildServiceProvider()
            .CreateAsyncScope();

        // Act
        await sut.RunStep(scope, step, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            step.Run(Arg.Any<CancellationToken>());
            hook1.PostStep(Arg.Any<StepExecutionSummary>(), Arg.Any<CancellationToken>());
            hook2.PostStep(Arg.Any<StepExecutionSummary>(), Arg.Any<CancellationToken>());
        });
    }
}
