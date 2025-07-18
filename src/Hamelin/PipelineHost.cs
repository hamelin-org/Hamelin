using Microsoft.Extensions.Hosting;

namespace Hamelin;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="lifetime">The application lifetime.</param>
/// <param name="stepCollection">The collection of pipeline steps to run.</param>
internal class PipelineHost(
    IHostApplicationLifetime lifetime,
    PipelineStepCollection stepCollection
) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Run each step in the pipeline.
        var steps = stepCollection.GetSteps();
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
