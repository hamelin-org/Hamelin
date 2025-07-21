using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="lifetime">The application lifetime.</param>
/// <param name="scopeFactory">The factory that will be used to scope each execution of the pipeline.</param>
/// <param name="options">Options to configure the pipeline execution</param>
internal class PipelineHost(
    IHostApplicationLifetime lifetime,
    IServiceScopeFactory scopeFactory,
    IOptions<PipelineHostOptions> options
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RunPipeline(cancellationToken);
        }
        finally
        {
            if (options.Value.StopApplicationOnCompletion)
            {
                // Exit the application gracefully now that the pipeline has run.
                lifetime.StopApplication();
            }
        }
    }

    private async Task RunPipeline(CancellationToken cancellationToken)
    {
        // Resolve the steps from a scoped service provider.
        await using var scope = scopeFactory.CreateAsyncScope();
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();

        // Run each step in the pipeline.
        foreach (var step in steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await step.Run(cancellationToken);
        }
    }
}
