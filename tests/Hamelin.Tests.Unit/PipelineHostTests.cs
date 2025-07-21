using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Tests.Unit;

public class PipelineHostTests
{
    [Fact]
    public async Task StartAsync_StopsApplicationWhenFinished()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var provider = Substitute.For<IPipelineStepProvider>();

        var host = new PipelineHost(lifetime, provider);

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        lifetime.Received().StopApplication();
    }

    [Fact]
    public async Task StartAsync_WithSteps_RunsStepsInOrder()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var provider = Substitute.For<IPipelineStepProvider>();
        provider.GetSteps().Returns([step1, step2, step3]);

        var host = new PipelineHost(lifetime, provider);

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
}
