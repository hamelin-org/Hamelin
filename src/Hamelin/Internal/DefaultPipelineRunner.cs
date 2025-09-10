using Hamelin.Hooks;
using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

internal class DefaultPipelineRunner(
    ILogger<DefaultPipelineRunner> logger,
    IOptions<PipelineExecutionOptions> options,
    IServiceScopeFactory scopeFactory,
    IPipelineStepRunner stepRunner
) : IPipelineRunner
{
    public async Task<PipelineExecutionSummary> RunPipeline(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running pipeline...");

        // Resolve the steps from a scoped service provider.
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IPipelineContext>();

        await RunPrePipelineHooks(scope, cancellationToken);
        var results = await RunSteps(scope, cancellationToken);
        var summary = new PipelineExecutionSummary(options, context, results, cancellationToken);
        await RunPostPipelineHooks(scope, summary, cancellationToken);

        logger.LogInformation("Pipeline finished with exit code {ExitCode}", summary.ExitCode);
        return summary;
    }

    private async Task RunPrePipelineHooks(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPrePipelineHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No pre-pipeline hooks registered.");
            return;
        }

        var args = new PrePipelineHookArgs();

        logger.LogInformation("Running pre-pipeline hooks...");
        foreach (var hook in hooks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Aborting pre-pipeline hooks due to cancellation request.");
                break;
            }

            try
            {
                await hook.PrePipeline(args, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error during pre-step hook. Continuing...");
            }
        }

        logger.LogInformation("Pre-pipeline hooks completed.");
    }

    private async Task RunPostPipelineHooks(AsyncServiceScope scope, PipelineExecutionSummary summary, CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPostPipelineHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No post-pipeline hooks registered.");
            return;
        }

        var args = new PostPipelineHookArgs
        {
            ExitCode = summary.ExitCode,
            Steps = summary.Steps
        };

        logger.LogInformation("Running post-pipeline hooks...");
        foreach (var hook in hooks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Aborting post-pipeline hooks due to cancellation request.");
                break;
            }

            try
            {
                await hook.PostPipeline(args, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error during post-step hook. Continuing...");
            }
        }

        logger.LogInformation("Post-pipeline hooks completed.");
    }

    private async Task<IEnumerable<StepExecutionSummary>> RunSteps(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        List<StepExecutionSummary> summaries = [];
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();
        foreach (var step in steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                summaries.Add(new StepExecutionSummary(step.GetDisplayName(), PipelineStepResult.StoppedAfterCancel));
                break;
            }

            var summary = await stepRunner.RunStep(scope, step, cancellationToken);
            summaries.Add(summary);
            if (!summary.Result.Continue)
            {
                logger.LogInformation("Step resulted in non-continuation. Aborting pipeline.");
                break;
            }
        }

        return summaries.ToArray();
    }
}
