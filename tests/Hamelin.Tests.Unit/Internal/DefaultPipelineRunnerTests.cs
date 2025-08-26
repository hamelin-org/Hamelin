using Hamelin.Hooks;
using Hamelin.Tests.Unit.Helpers;

namespace Hamelin.Tests.Unit.Internal;

public class DefaultPipelineRunnerTests
{
    public DefaultPipelineRunnerTests()
    {
        Environment.ExitCode = 0;
    }

    [Fact]
    public async Task RunPipeline_WithSteps_RunsStepsInOrder()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner([step1, step2, step3]);

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
            step3.Run(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_WithStopOnUnhandledException_StopsOnUnhandledException()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
        });
        await step3.DidNotReceive().Run(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunPipeline_WithStopAfterAllSteps_StopsAfterAllSteps()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
            step3.Run(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_AutoExitCodesAndTokenCancelled_SetsExitCodeToCancelled()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner([step1]);

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        await sut.RunPipeline(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedAfterCancel);
    }

    [Fact]
    public async Task RunPipeline_AutoExitCodesAndStopOnUnhandledException_SetsExitCodeToStoppedOnError()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }

    [Fact]
    public async Task RunPipeline_AutoExitCodesAndStopAfterAllSteps_SetsExitCodeToContinuedAfterError()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.ContinuedAfterError);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndTokenCancelled_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1],
            configure: options => options.EnableAutomaticExitCodes = false
        );

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        await sut.RunPipeline(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndStopOnUnhandledException_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException;
            });

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_NoAutoExitCodesAndStopAfterAllSteps_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2, step3],
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps;
            });

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task RunPipeline_CustomExitCode_SetsExitCodeToCustomExitCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();

        var step1 = PipelineStepHelpers.CreateMock();
        step1
            .When(s => s.Run(Arg.Any<CancellationToken>()))
            .Do((i) =>
            {
                context.ExitCode = 1234;
            });

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1],
            context: context
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Environment.ExitCode.ShouldBe(1234);
    }

    [Fact]
    public async Task RunPipeline_CustomExitCode_DoesNotOverrideAutomaticCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();

        var step1 = PipelineStepHelpers.CreateMock();
        step1
            .When(s => s.Run(Arg.Any<CancellationToken>()))
            .Do((i) =>
            {
                context.ExitCode = 1234;
            });
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step1, step2],
            context: context
        );

        // Act
        await sut.RunPipeline(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }

    [Fact]
    public async Task RunPipeline_WithPrePipelineHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPrePipelineHook>();
        var hook2 = Substitute.For<IPrePipelineHook>();
        var step = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            prePipelineHooks: [hook1, hook2],
            steps: [step],
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
            step.Run(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RunPipeline_WithPostPipelineHooks_RunsHooks()
    {
        // Arrange
        var hook1 = Substitute.For<IPostPipelineHook>();
        var hook2 = Substitute.For<IPostPipelineHook>();
        var step = PipelineStepHelpers.CreateMock();

        var sut = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [step],
            postPipelineHooks: [hook1, hook2],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => sut.RunPipeline(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Received.InOrder(() =>
        {
            step.Run(Arg.Any<CancellationToken>());
            hook1.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
            hook2.PostPipeline(Arg.Any<PipelineExecutionSummary>(), Arg.Any<CancellationToken>());
        });
    }
}
