namespace Hamelin.Tests.Unit;

public class PipelineApplicationTests
{
    [Fact]
    public async Task CreateBuilder_ShouldReturnPipelineApplicationBuilder()
    {
        // Arrange
        string[] args = ["arg1", "arg2"];

        // Act
        var builder = PipelineApplication.CreateBuilder(args);
        var pipeline = builder.Build();
        var act = () => pipeline.StartAsync(CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
