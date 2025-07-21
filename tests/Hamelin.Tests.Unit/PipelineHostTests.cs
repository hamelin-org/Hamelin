using Hamelin.Steps;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;
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
        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        var host = new PipelineHost(lifetime, scopeFactory);

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

        var stepProvider = Substitute.For<IPipelineStepProvider>();
        stepProvider.GetSteps().Returns([step1, step2, step3]);

        var services = new ServiceCollection()
            .AddSingleton(stepProvider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        var host = new PipelineHost(lifetime, scopeFactory);

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
