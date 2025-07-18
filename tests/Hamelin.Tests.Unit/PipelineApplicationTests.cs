using Microsoft.Extensions.Hosting;

namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task RunAsync_DoesNotThrowOrHang()
    {
        // Arrange
        var builder = PipelineApplication.CreateBuilder();
        var pipeline = builder.Build();

        // Act
        var act = () => pipeline.RunAsync();

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
