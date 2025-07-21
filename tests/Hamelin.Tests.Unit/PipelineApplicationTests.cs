using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task RunAsync_EmptyApplication_DoesNotThrowOrHang()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        builder.AddStep<TestPipelineStep>();

        var pipeline = builder.Build();

        pipeline.UseStep<TestPipelineStep>();

        // Act
        var act = () => pipeline.RunAsync();

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public void UseStep_AddsStepToCollector()
    {
        // Arrange
        var collector = Substitute.For<IPipelineStepCollector>();

        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddSingleton(collector);

        var pipeline = builder.Build();

        // Act
        pipeline.UseStep<TestPipelineStep>();

        // Assert
        collector.Received().AddStep<TestPipelineStep>();
    }
}

class TestPipelineStep : IPipelineStep
{
    public Task Run(CancellationToken cancellationToken = default)
    {
        // Simulate some work
        return Task.CompletedTask;
    }
}
