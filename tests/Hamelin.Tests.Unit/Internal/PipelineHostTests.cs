using Hamelin.Internal;
using Hamelin.Steps;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Internal;

public class PipelineHostTests
{
    [Fact]
    public async Task StartAsync_WithStopApplicationOnCompletion_StopsApplicationWhenFinished()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PipelineHost>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var provider = Substitute.For<IPipelineStepProvider>();
        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new()
        {
            StopApplicationOnCompletion = true
        };

        var host = new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        lifetime.Received().StopApplication();
    }

    [Fact]
    public async Task StartAsync_WithoutStopApplicationOnCompletion_DoesNotStopApplicationWhenFinished()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PipelineHost>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var provider = Substitute.For<IPipelineStepProvider>();
        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new()
        {
            StopApplicationOnCompletion = false
        };

        var host = new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));

        // Act
        await host.StartAsync(CancellationToken.None);

        // Assert
        lifetime.DidNotReceive().StopApplication();
    }

    [Fact]
    public async Task StartAsync_WithSteps_RunsStepsInOrder()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PipelineHost>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        var step3 = PipelineStepHelpers.CreateMock();

        var provider = Substitute.For<IPipelineStepProvider>();
        provider.GetSteps().Returns([step1, step2, step3]);

        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new();

        var host = new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));

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
    public async Task StartAsync_WithStopOnUnhandledException_StopsOnUnhandledsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PipelineHost>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var provider = Substitute.For<IPipelineStepProvider>();
        provider.GetSteps().Returns([step1, step2, step3]);

        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new()
        {
            TerminationMode = PipelineTerminationMode.StopOnUnhandledException
        };

        var host = new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));

        // Act
        var act = () => host.StartAsync(CancellationToken.None);


        // Assert
        await act.ShouldThrowAsync<Exception>();
        Received.InOrder(() =>
        {
            step1.Run(Arg.Any<CancellationToken>());
            step2.Run(Arg.Any<CancellationToken>());
        });
        await step3.DidNotReceive().Run(Arg.Any<CancellationToken>());
    }

    [Fact] public async Task StartAsync_WithStopAfterAllSteps_StopsAfterAllSteps()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PipelineHost>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var step1 = PipelineStepHelpers.CreateMock();
        var step2 = PipelineStepHelpers.CreateMock();
        step2.Run(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test"));
        var step3 = PipelineStepHelpers.CreateMock();

        var provider = Substitute.For<IPipelineStepProvider>();
        provider.GetSteps().Returns([step1, step2, step3]);

        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new()
        {
            TerminationMode = PipelineTerminationMode.StopAfterAllSteps
        };

        var host = new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));

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
}
