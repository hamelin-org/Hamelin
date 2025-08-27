using Hamelin.Hooks;
using Hamelin.Internal;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Tests.Unit.Internal;

public class DefaultPipelineRunnerTests
{
    [Fact]
    public async Task RunPipeline_WithSteps_RunsStepsInOrder()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step1, Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step3, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_StoppedOnError_StopsOnUnhandledException()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.StoppedOnError(new Exception()) });

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step1, Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>());
        });
        await stepRunner.DidNotReceive().RunStep(Arg.Any<AsyncServiceScope>(), step3, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunPipeline_ContinuedAfterError_StopsAfterAllSteps()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.ContinuedAfterError(new Exception()) });

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step1, Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step3, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_TokenCancelled_ReturnsCorrectExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner([step1]);

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var summary = await sut.RunPipeline(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        summary.ExitCode.ShouldBe(PipelineExitCodes.StoppedAfterCancel);
    }

    [Fact]
    public async Task RunPipeline_StopOnUnhandledException_ReturnsCorrectExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.StoppedOnError(new Exception()) }); ;

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }

    [Fact]
    public async Task RunPipeline_ContinueOnError_ReturnsCorrectExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.ContinuedAfterError(new Exception()) }); ;

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(PipelineExitCodes.ContinuedAfterError);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndTokenCancelled_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1],
            stepRunner: stepRunner,
            configure: options => options.EnableAutomaticExitCodes = false
        );

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var summary = await sut.RunPipeline(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        summary.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndStopOnUnhandledException_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.StoppedOnError(new Exception()) }); ;

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException;
            });

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndStopAfterAllSteps_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.ContinuedAfterError(new Exception()) }); ;

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            stepRunner: stepRunner,
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps;
            });

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_CustomExitCode_SetsExitCodeToCustomExitCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();
        var step1 = PipelineStepHelpers.CreateMock();

        var stepRunner = PipelineStepRunnerHelpers.CreateMock();
        stepRunner
            .When(s => s.RunStep(Arg.Any<AsyncServiceScope>(), step1, Arg.Any<CancellationToken>()))
            .Do(_ => context.ExitCode = 1234);

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1],
            stepRunner: stepRunner,
            context: context
        );

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(1234);
    }

    [Fact]
    public async Task RunPipeline_CustomExitCode_DoesNotOverrideAutomaticCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();

        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        stepRunner
            .When(s => s.RunStep(Arg.Any<AsyncServiceScope>(), step1, Arg.Any<CancellationToken>()))
            .Do(_ => context.ExitCode = 1234);

        stepRunner
            .RunStep(Arg.Any<AsyncServiceScope>(), step2, Arg.Any<CancellationToken>())
            .Returns(new StepExecutionSummary { StepName = "", Result = PipelineStepResult.StoppedOnError(new Exception()) }); ;

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2],
            stepRunner: stepRunner,
            context: context
        );

        // Act
        var summary = await sut.RunPipeline(CancellationToken.None);

        // Assert
        summary.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }

    [Fact]
    public async Task RunPipeline_WithPrePipelineHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPrePipelineHook>();
        var hook2 = Substitute.For<IPrePipelineHook>();
        var step = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            prePipelineHooks: [hook1, hook2],
            steps: [step],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            hook1.PrePipeline(Arg.Any<CancellationToken>());
            hook2.PrePipeline(Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_WithPostPipelineHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPostPipelineHook>();
        var hook2 = Substitute.For<IPostPipelineHook>();
        var step = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step],
            postPipelineHooks: [hook1, hook2],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step, Arg.Any<CancellationToken>());
            hook1.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
            hook2.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_PrePipelineHookError_StillRunsAllHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPrePipelineHook>();
        hook1.PrePipeline(Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        var hook2 = Substitute.For<IPrePipelineHook>();
        var step = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            prePipelineHooks: [hook1, hook2],
            steps: [step],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            hook1.PrePipeline(Arg.Any<CancellationToken>());
            hook2.PrePipeline(Arg.Any<CancellationToken>());
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_PostPipelineHookError_StillRunsAllHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPostPipelineHook>();
        hook1.PostPipeline(Arg.Any<PipelineExecutionSummary>(),Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        var hook2 = Substitute.For<IPostPipelineHook>();
        var step = PipelineStepHelpers.CreateMock();
        var stepRunner = PipelineStepRunnerHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step],
            postPipelineHooks: [hook1, hook2],
            stepRunner: stepRunner,
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            stepRunner.RunStep(Arg.Any<AsyncServiceScope>(), step, Arg.Any<CancellationToken>());
            hook1.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
            hook2.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
        });
    }
}
