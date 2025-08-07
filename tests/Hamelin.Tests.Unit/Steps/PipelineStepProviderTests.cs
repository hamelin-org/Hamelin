using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Tests.Unit.Steps;

public class PipelineStepProviderTests
{
    [Fact]
    public void GetSteps_WithSteps_ReturnsCorrectSteps()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddStep<DummyStep1>()
            .AddStep<DummyStep2>()
            .BuildServiceProvider();

        var collection = new PipelineStepCollection();
        collection.AddStep<DummyStep1>();
        collection.AddStep<DummyStep2>();

        var provider = new PipelineStepProvider(collection, services);

        // Act
        var steps = provider.GetSteps().ToList();

        // Assert
        steps.ShouldBeOfTypes(typeof(DummyStep1), typeof(DummyStep2));
    }

    private class DummyStep1 : IPipelineStep
    {
        public Task Run(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class DummyStep2 : IPipelineStep
    {
        public Task Run(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
