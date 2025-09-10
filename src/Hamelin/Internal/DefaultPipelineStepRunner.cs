using Hamelin.Hooks;
using Hamelin.Logging;
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
        string displayName = step.GetDisplayName();

        var attribs = new LogAttributes { StepName = displayName };
        using var loggingScope = logger.BeginScope(attribs);

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Aborting pipeline step due to cancellation request.");
            return new StepExecutionSummary(displayName, PipelineStepResult.StoppedAfterCancel);
        }

        await RunPreStepHooks(scope, attribs);

        var result = await RunStepCore(step, cancellationToken);
        var summary = new StepExecutionSummary(displayName, result);

        await RunPostStepHooks(scope, summary);

        return summary;
    }

    private async Task RunPreStepHooks(AsyncServiceScope scope, LogAttributes attributes)
    {
        var hooks = scope.ServiceProvider.GetServices<IPreStepHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No pre-step hooks registered.");
            return;
        }

        var args = new PreStepHookArgs
        {
            StepName = attributes.StepName,
        };

        logger.LogDebug("Running pre-step hooks...");
        foreach (var hook in hooks)
        {
            await RunPreStepHook(hook, args);
        }

        logger.LogDebug("Pre-step hooks complete.");
    }

    private async Task RunPreStepHook(IPreStepHook hook, PreStepHookArgs args)
    {
        try
        {
            await hook.PreStep(args, CancellationToken.None);
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

        var args = new PostStepHookArgs
        {
            StepName = summary.StepName,
            Result = summary.Result
        };

        logger.LogDebug("Running post-step hooks...");
        foreach (var hook in hooks)
        {
            await RunPostStepHook(hook, args);
        }

        logger.LogDebug("Post-step hooks complete.");
    }

    private async Task RunPostStepHook(IPostStepHook hook, PostStepHookArgs args)
    {
        try
        {
            await hook.PostStep(args, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error during post-step hook. Continuing...");
        }
    }
}
