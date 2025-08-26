using Hamelin.Hooks;
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

    // TODO: Test for running pipeline.
}
