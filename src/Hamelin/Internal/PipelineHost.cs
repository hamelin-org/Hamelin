using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="logger">The logger for the pipeline host.</param>
/// <param name="lifetime">The application lifetime.</param>
/// <param name="scopeFactory">The factory that will be used to scope each execution of the pipeline.</param>
/// <param name="options">Options to configure the pipeline execution</param>
internal class PipelineHost(
    ILogger<PipelineHost> logger,
    IHostApplicationLifetime lifetime,
    IServiceScopeFactory scopeFactory,
    IOptions<PipelineExecutionOptions> options
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

            try
            {
                await step.Run(cancellationToken);
            }
            catch (Exception ex)
            {
                switch (options.Value.TerminationMode)
                {
                    case PipelineTerminationMode.StopOnUnhandledException:
                        logger.LogCritical(ex, "Unhandled error during pipeline execution.");
                        lifetime.StopApplication();
                        throw;
                    case PipelineTerminationMode.StopAfterAllSteps:
                        logger.LogError(ex, "Unhandled error during pipeline execution.");
                        break;
                    default:
                        throw;
                }
            }
        }
    }
}
