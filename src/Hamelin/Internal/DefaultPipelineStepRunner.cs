using Hamelin.Hooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

internal class DefaultPipelineStepRunner(
    ILogger<DefaultPipelineStepRunner> logger,
    IOptions<PipelineExecutionOptions> options
) : IPipelineStepRunner
{
    public async Task<StepExecutionSummary> RunStep(AsyncServiceScope scope, IPipelineStep step, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Aborting pipeline step due to cancellation request.");
            return StepExecutionSummary.FromStep(step, PipelineStepResult.StoppedAfterCancel);
        }

        await RunPreStepHooks(scope);

        var result = await RunStepCore(step, cancellationToken);
        var summary = StepExecutionSummary.FromStep(step, result);

        await RunPostStepHooks(scope, summary);

        return summary;
    }

    private async Task RunPreStepHooks(AsyncServiceScope scope)
    {
        var hooks = scope.ServiceProvider.GetServices<IPreStepHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No pre-step hooks registered.");
            return;
        }

        logger.LogDebug("Running pre-step hooks...");
        foreach (var hook in hooks)
        {
            await RunPreStepHook(hook);
        }

        logger.LogDebug("Pre-step hooks complete.");
    }

    private async Task RunPreStepHook(IPreStepHook hook)
    {
        try
        {
            await hook.PreStep(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error during pre-step hook. Continuing...");
        }
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
                    return PipelineStepResult.StoppedOnError(ex);
            }
        }
    }

    private async Task RunPostStepHooks(AsyncServiceScope scope, StepExecutionSummary summary)
    {
        var hooks = scope.ServiceProvider.GetServices<IPostStepHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No post-step hooks registered.");
            return;
        }

        logger.LogDebug("Running post-step hooks...");
        foreach (var hook in hooks)
        {
            await RunPostStepHook(hook, summary);
        }

        logger.LogDebug("Post-step hooks complete.");
    }

    private async Task RunPostStepHook(IPostStepHook hook, StepExecutionSummary summary)
    {
        try
        {
            await hook.PostStep(summary, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error during post-step hook. Continuing...");
        }
    }
}
