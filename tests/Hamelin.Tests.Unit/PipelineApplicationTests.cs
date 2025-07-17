namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task StartAsync_DoesNotThrow()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        var pipeline = builder.Build();

        // Act
        var act = () => pipeline.StartAsync(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
