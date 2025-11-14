using Hamelin.Internal;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Internal;

public class PipelineHostTests
{
    public PipelineHostTests()
    {
        Environment.ExitCode = 0;
    }

    [Fact]
    public async Task StartAndStopAsync_WithSetEnvironmentExitCode_RunsPipelineAndSetsExitCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();
        context.ExitCode.Returns(1234);

        var options = Options.Create(new PipelineExecutionOptions
        {
            SetEnvironmentExitCodeOnCompletion = true
        });
        var summary = new PipelineExecutionSummary(options, context, [], CancellationToken.None);

        var runner = Substitute.For<IPipelineRunner>();
        runner
            .RunPipeline(Arg.Any<CancellationToken>())
            .Returns(summary);

        var summaryStore = new PipelineExecutionSummaryStore();

        var sut = PipelineHostHelpers.CreateHost(runner: runner, summaryStore: summaryStore);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!.WaitAsync(CancellationToken.None);

        // Assert
        await runner.Received().RunPipeline(Arg.Any<CancellationToken>());
        Environment.ExitCode.ShouldBe(1234);
        summaryStore.Summary.ShouldBe(summary);
    }

    [Fact]
    public async Task StartAndStopAsync_WithoutSetEnvironmentExitCode_RunsPipelineAndDoesNotSetExitCode()
    {
        // Arrange
        var context = Substitute.For<IPipelineContext>();
        context.ExitCode.Returns(1234);

        var sut = PipelineHostHelpers.CreateHost(configure: options => options.SetEnvironmentExitCodeOnCompletion = false);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!.WaitAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task StartAndStopAsync_WithStopApplicationOnCompletion_StopsApplicationWhenFinished()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var sut = PipelineHostHelpers.CreateHost(
            configure: options => options.StopApplicationOnCompletion = true,
            lifetime: lifetime
        );

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!.WaitAsync(CancellationToken.None);

        // Assert
        lifetime.Received().StopApplication();
    }

    [Fact]
    public async Task StartAndStopAsync_WithoutStopApplicationOnCompletion_DoesNotStopApplicationWhenFinished()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var sut = PipelineHostHelpers.CreateHost(
            configure: options => options.StopApplicationOnCompletion = false,
            lifetime: lifetime
        );

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!.WaitAsync(CancellationToken.None);

        // Assert
        lifetime.DidNotReceive().StopApplication();
    }
}
