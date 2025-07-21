using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Internal;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="lifetime">The application lifetime.</param>
/// <param name="scopeFactory">The factory that will be used to scope each execution of the pipeline.</param>
internal class PipelineHost(
    IHostApplicationLifetime lifetime,
    IServiceScopeFactory scopeFactory
) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolve the steps from a scoped service provider.
        await using var scope = scopeFactory.CreateAsyncScope();
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();

        // Run each step in the pipeline.
        foreach (var step in steps)
        {
            await step.Run(cancellationToken);
        }

        // Exit the application gracefully now that the pipeline has run.
        lifetime.StopApplication();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
