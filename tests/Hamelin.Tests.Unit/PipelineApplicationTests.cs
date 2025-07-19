using Microsoft.Extensions.Hosting;

namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task RunAsync_DoesNotThrowOrHang()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        builder.AddStep<TestPipelineStep>();

        var pipeline = builder.Build();

        pipeline.RunStep<TestPipelineStep>();

        // Act
        var act = () => pipeline.RunAsync();

        // Assert
        await act.ShouldNotThrowAsync();
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
