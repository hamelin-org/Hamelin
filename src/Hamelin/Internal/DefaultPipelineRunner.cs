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
            logger.LogInformation("No pre-pipeline hooks registered.");
            return;
        }

        logger.LogInformation("Running pre-pipeline hooks...");
        foreach (var hook in hooks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Aborting pre-pipeline hooks due to cancellation request.");
                return;
            }

            await hook.PrePipeline(cancellationToken);
        }

        logger.LogInformation("Pre-pipeline hooks completed successfully.");
    }

    private async Task RunPostPipelineHooks(AsyncServiceScope scope, PipelineExecutionSummary summary, CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPostPipelineHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogInformation("No post-pipeline hooks registered.");
            return;
        }

        logger.LogInformation("Running post-pipeline hooks...");
        foreach (var hook in hooks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Aborting post-pipeline hooks due to cancellation request.");
                return;
            }

            await hook.PostPipeline(summary, cancellationToken);
        }

        logger.LogInformation("Post-pipeline hooks completed successfully.");
    }

    private async Task<IEnumerable<PipelineStepResult>> RunSteps(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        List<PipelineStepResult> summaries = [];
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();
        foreach (var step in steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                summaries.Add(PipelineStepResult.StoppedAfterCancel);
                break;
            }

            var result = await stepRunner.RunStep(scope, step, cancellationToken);
            summaries.Add(result);
            if (!result.Continue)
            {
                logger.LogInformation("Step resulted in non-continuation. Aborting pipeline.");
                break;
            }
        }

        return summaries.ToArray();
    }
}
