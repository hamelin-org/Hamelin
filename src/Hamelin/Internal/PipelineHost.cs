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
            int result = await RunPipeline(cancellationToken);
            Environment.ExitCode = result;
        }
        catch (Exception)
        {
            // The only way we reach this is when the pipeline terminates early due to an unhandled error.
            if (options.Value.EnableAutomaticExitCodes)
            {
                Environment.ExitCode = PipelineExitCodes.StoppedOnError;
            }
            throw;
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

    private async Task<int> RunPipeline(CancellationToken cancellationToken)
    {
        // Resolve the steps from a scoped service provider.
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IPipelineContext>();
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();

        int automaticExitCode = PipelineExitCodes.Success;
        foreach (var step in steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                automaticExitCode = PipelineExitCodes.StoppedAfterCancel;
                break;
            }

            try
            {
                await step.Run(cancellationToken);
            }
            catch (Exception ex)
            {
                switch (options.Value.TerminationMode)
                {
                    case PipelineTerminationMode.StopAfterAllSteps:
                        logger.LogError(ex, "Unhandled error during pipeline execution. Continuing...");
                        automaticExitCode = PipelineExitCodes.ContinuedAfterError;
                        break;
                    case PipelineTerminationMode.StopOnUnhandledException: // This is the default behaviour, so fall through to default case
                    default:
                        logger.LogCritical(ex, "Unhandled error during pipeline execution. Exiting.");
                        lifetime.StopApplication();
                        throw;
                }
            }
        }

        if (options.Value.EnableAutomaticExitCodes && automaticExitCode != PipelineExitCodes.Success)
        {
            return automaticExitCode;
        }
        return context.ExitCode ?? PipelineExitCodes.Success;
    }
}
