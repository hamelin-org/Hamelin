using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task RunAsync_EmptyApplication_DoesNotThrowOrHang()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddStep<TestPipelineStep>();

        var pipeline = builder.Build();

        pipeline.UseStep<TestPipelineStep>();

        // Act
        var act = () => pipeline.RunAsync();

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task RunWithExitCodeAsync_EmptyApplication_DoesNotThrowOrHang()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddStep<TestPipelineStep>();

        var pipeline = builder.Build();

        pipeline.UseStep<TestPipelineStep>();

        // Act
        int exitCode = await pipeline.RunWithExitCodeAsync(TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task RunWithExitCodeAsync_WithCustomExitCode_ReturnsExitCode()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddStep<SetExitCodeTestPipelineStep>();

        var pipeline = builder.Build();

        pipeline.UseStep<SetExitCodeTestPipelineStep>();

        // Act
        int exitCode = await pipeline.RunWithExitCodeAsync(TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1234);
    }

    [Fact]
    public void UseStep_AddsStepToCollector()
    {
        // Arrange
        var collector = Substitute.For<IPipelineStepCollection>();

        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddSingleton(collector);

        var pipeline = builder.Build();

        // Act
        pipeline.UseStep(typeof(TestPipelineStep));

        // Assert
        collector.Received().AddStep(typeof(TestPipelineStep));
    }

    [Fact]
    public void UseStepGeneric_AddsStepToCollector()
    {
        // Arrange
        var collector = Substitute.For<IPipelineStepCollection>();

        var builder = PipelineApplication.CreateBuilder();
        builder.Services.AddSingleton(collector);

        var pipeline = builder.Build();

        // Act
        pipeline.UseStep<TestPipelineStep>();

        // Assert
        collector.Received().AddStep(typeof(TestPipelineStep));
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

class SetExitCodeTestPipelineStep(IPipelineContext context) : IPipelineStep
{
    public Task Run(CancellationToken cancellationToken = default)
    {
        context.ExitCode = 1234;
        return Task.CompletedTask;
    }
}
