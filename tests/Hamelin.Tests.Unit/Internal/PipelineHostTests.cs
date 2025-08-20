using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Tests.Unit.Internal;

public class PipelineHostTests
{
    public PipelineHostTests()
    {
        Environment.ExitCode = 0;
    }

    [Fact]
    public async Task StartAsync_WithStopApplicationOnCompletion_StopsApplicationWhenFinished()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var host = PipelineHostHelpers.CreateHost(
            configure: options => options.StopApplicationOnCompletion = true,
            lifetime: lifetime
        );

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        lifetime.Received().StopApplication();
    }

    [Fact]
    public async Task StartAsync_WithoutStopApplicationOnCompletion_DoesNotStopApplicationWhenFinished()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var host = PipelineHostHelpers.CreateHost(
            configure: options => options.StopApplicationOnCompletion = false,
            lifetime: lifetime
        );

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        lifetime.DidNotReceive().StopApplication();
    }

    [Fact]
    public async Task StartAsync_WithSteps_RunsStepsInOrder()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost([step1, step2, step3]);

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
            step3.Run(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task StartAsync_WithStopOnUnhandledException_StopsOnUnhandledException()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
        });
        await step3.DidNotReceive().Run(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WithStopAfterAllSteps_StopsAfterAllSteps()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => host.StartAsync(CancellationToken.None);

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
    public async Task StartAsync_AutoExitCodesAndTokenCancelled_SetsExitCodeToCancelled()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost([step1]);

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        await host.StartAsync(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedAfterCancel);
    }

    [Fact]
    public async Task StartAsync_AutoExitCodesAndStopOnUnhandledException_SetsExitCodeToStoppedOnError()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        );

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }

    [Fact]
    public async Task StartAsync_AutoExitCodesAndStopAfterAllSteps_SetsExitCodeToContinuedAfterError()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options => options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        );

        // Act
        var act = () => host.StartAsync(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Environment.ExitCode.ShouldBe(PipelineExitCodes.ContinuedAfterError);
    }

    [Fact]
    public async Task StartAsync_NoAutoExitCodesAndTokenCancelled_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1],
            options => options.EnableAutomaticExitCodes = false
        );

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        await host.StartAsync(cts.Token);

        // Assert
        await step1.DidNotReceive().Run(Arg.Any<CancellationToken>());
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task StartAsync_NoAutoExitCodesAndStopOnUnhandledException_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopOnUnhandledException;
            });

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task StartAsync_NoAutoExitCodesAndStopAfterAllSteps_DoesNotSetExitCode()
    {
        // Arrange
        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2, step3],
            configure: options =>
            {
                options.EnableAutomaticExitCodes = false;
                options.TerminationMode = PipelineTerminationMode.StopAfterAllSteps;
            });

        // Act
        var act = () => host.StartAsync(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Environment.ExitCode.ShouldBe(PipelineExitCodes.Success);
    }

    [Fact]
    public async Task StartAsync_CustomExitCode_SetsExitCodeToCustomExitCode()
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

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1],
            context: context
        );

        // Act
        var act = () => host.StartAsync(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        Environment.ExitCode.ShouldBe(1234);
    }

    [Fact]
    public async Task StartAsync_CustomExitCode_DoesNotOverrideAutomaticCode()
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

        var host = PipelineHostHelpers.CreateHost(
            steps: [step1, step2],
            context: context
        );

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(PipelineExitCodes.StoppedOnError);
    }
}
