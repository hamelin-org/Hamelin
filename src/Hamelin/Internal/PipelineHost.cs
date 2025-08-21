using Hamelin.Hooks;
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
            var summary = await RunPipeline(cancellationToken);
            Environment.ExitCode = summary.ExitCode;
        }
        finally
        {
            if (options.Value.StopApplicationOnCompletion)
            {
                // Exit the application gracefully now that the pipeline has run.
                logger.LogInformation("Requesting application stop...");
                lifetime.StopApplication();
            }
        }
    }

    private async Task<PipelineExecutionSummary> RunPipeline(CancellationToken cancellationToken)
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
        }
        else
        {
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
    }

    private async Task RunPostPipelineHooks(AsyncServiceScope scope, PipelineExecutionSummary summary, CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPostPipelineHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogInformation("No post-pipeline hooks registered.");
        }
        else
        {
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
    }

    private async Task<IEnumerable<PipelineStepResult>> RunSteps(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        List<PipelineStepResult> results = [];
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();
        foreach (var step in steps)
        {
            var result = await RunStep(step, cancellationToken);
            results.Add(result);
            if (!result.Continue)
            {
                logger.LogInformation("Step resulted in non-continuation. Aborting pipeline.");
                break;
            }
        }

        return results.ToArray();
    }

    private async Task<PipelineStepResult> RunStep(IPipelineStep step, CancellationToken cancellationToken)
    {
        var stepType = step.GetType();
        string stepName = stepType.Name;

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Aborting pipeline steps due to cancellation request.");
            return PipelineStepResult.StoppedAfterCancel;
        }

        try
        {
            logger.LogInformation("Running {StepName}...", stepName);
            await step.Run(cancellationToken);
            logger.LogInformation("{StepName} completed successfully.", stepName);
            return PipelineStepResult.Successful;
        }
        catch (Exception ex)
        {
            switch (options.Value.TerminationMode)
            {
                case PipelineTerminationMode.StopAfterAllSteps:
                    logger.LogError(ex, "Unhandled error during pipeline execution of step {StepName}. Continuing...", stepName);
                    return PipelineStepResult.ContinuedAfterError(ex);
                case PipelineTerminationMode.StopOnUnhandledException: // This is the default behaviour, so fall through to default case
                default:
                    logger.LogCritical(ex, "Unhandled error during pipeline execution of step {StepName}. Exiting.", stepName);
                    lifetime.StopApplication();
                    return PipelineStepResult.StoppedOnError(ex);
            }
        }
    }
}
