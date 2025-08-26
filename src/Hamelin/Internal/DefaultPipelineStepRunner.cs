using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

internal class DefaultPipelineStepRunner(
    ILogger<DefaultPipelineStepRunner> logger,
    IOptions<PipelineExecutionOptions> options,
    IHostApplicationLifetime lifetime
) : IPipelineStepRunner
{
    public async Task<PipelineStepResult> RunStep(AsyncServiceScope scope, IPipelineStep step, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Aborting pipeline step due to cancellation request.");
            return PipelineStepResult.StoppedAfterCancel;
        }

        var result = await RunStepCore(step, cancellationToken);

        return result;
    }

    private async Task<PipelineStepResult> RunStepCore(IPipelineStep step, CancellationToken cancellationToken)
    {
        try
        {
            await step.Run(cancellationToken);
            return PipelineStepResult.Successful;
        }
        catch (Exception ex)
        {
            switch (options.Value.TerminationMode)
            {
                case PipelineTerminationMode.StopAfterAllSteps:
                    logger.LogError(ex, "Unhandled error during step. Continuing...");
                    return PipelineStepResult.ContinuedAfterError(ex);
                // This is the default behaviour, so fall through to default case
                case PipelineTerminationMode.StopOnUnhandledException:
                default:
                    logger.LogCritical(ex, "Unhandled error during step. Exiting.");
                    lifetime.StopApplication();
                    return PipelineStepResult.StoppedOnError(ex);
            }
        }
    }
}
