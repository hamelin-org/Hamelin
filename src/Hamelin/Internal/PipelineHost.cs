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

    private async Task RunPostPipelineHooks(AsyncServiceScope scope, PipelineExecutionSummary summary,
        CancellationToken cancellationToken)
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

    private async Task<IEnumerable<StepExecutionSummary>> RunSteps(AsyncServiceScope scope,
        CancellationToken cancellationToken)
    {
        List<StepExecutionSummary> summaries = [];
        var stepProvider = scope.ServiceProvider.GetRequiredService<IPipelineStepProvider>();
        var steps = stepProvider.GetSteps();
        foreach (var step in steps)
        {
            var summary = await RunStep(scope, step, cancellationToken);
            summaries.Add(summary);
            if (!summary.Result.Continue)
            {
                logger.LogInformation("Step resulted in non-continuation. Aborting pipeline.");
                break;
            }
        }

        return summaries.ToArray();
    }

    private async Task<StepExecutionSummary> RunStep(AsyncServiceScope scope, IPipelineStep step, CancellationToken cancellationToken)
    {
        var stepType = step.GetType();
        string stepName = stepType.Name;

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Aborting pipeline steps due to cancellation request.");
            return new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.StoppedAfterCancel, };
        }

        StepExecutionSummary? summary = null;

        try
        {
            await RunPreStepHooks(scope, cancellationToken);
        }
        catch (Exception ex)
        {
            switch (options.Value.TerminationMode)
            {
                case PipelineTerminationMode.StopAfterAllSteps:
                    logger.LogError(ex, "Unhandled error during pre-step hook of step {StepName}. Continuing...", stepName);
                    summary = new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.ContinuedAfterError(ex) };
                    break;
                case PipelineTerminationMode.StopOnUnhandledException: // This is the default behaviour, so fall through to default case
                default:
                    logger.LogCritical(ex, "Unhandled error during pre-step hook of step {StepName}. Exiting.", stepName);
                    lifetime.StopApplication();
                    return new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.StoppedOnError(ex), };
            }
        }

        try
        {
            logger.LogInformation("Running {StepName}...", stepName);
            await step.Run(cancellationToken);
            logger.LogInformation("{StepName} completed successfully.", stepName);

            // Don't overwrite the summary if there was an issue during pre-step hooks.
            summary ??= new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.Successful, };
        }
        catch (Exception ex)
        {
            switch (options.Value.TerminationMode)
            {
                case PipelineTerminationMode.StopAfterAllSteps:
                    logger.LogError(ex, "Unhandled error during pipeline execution of step {StepName}. Continuing...", stepName);
                    summary = new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.ContinuedAfterError(ex) };
                    break;
                case PipelineTerminationMode.StopOnUnhandledException: // This is the default behaviour, so fall through to default case
                default:
                    logger.LogCritical(ex, "Unhandled error during pipeline execution of step {StepName}. Exiting.", stepName);
                    lifetime.StopApplication();
                    summary = new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.StoppedOnError(ex) };
                    break;
            }
        }

        try
        {
            await RunPostStepHooks(scope, summary, cancellationToken);
        }
        catch (Exception ex)
        {
            switch (options.Value.TerminationMode)
            {
                case PipelineTerminationMode.StopAfterAllSteps:
                    logger.LogError(ex, "Unhandled error during post-step hook of step {StepName}. Continuing...", stepName);
                    break;
                case PipelineTerminationMode.StopOnUnhandledException: // This is the default behaviour, so fall through to default case
                default:
                    logger.LogCritical(ex, "Unhandled error during post-step hook of step {StepName}. Exiting.", stepName);
                    lifetime.StopApplication();
                    return new StepExecutionSummary { StepName = stepName, Result = PipelineStepResult.StoppedOnError(ex), };
            }
        }

        return summary;
    }

    private async Task RunPreStepHooks(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPreStepHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No pre-step hooks registered.");
        }
        else
        {
            logger.LogDebug("Running pre-step hooks...");
            foreach (var hook in hooks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Aborting pre-step hooks due to cancellation request.");
                    return;
                }

                await hook.PreStep(cancellationToken);
            }

            logger.LogDebug("Pre-step hooks completed successfully.");
        }
    }

    private async Task RunPostStepHooks(AsyncServiceScope scope, StepExecutionSummary summary,
        CancellationToken cancellationToken)
    {
        var hooks = scope.ServiceProvider.GetServices<IPostStepHook>().ToList();
        if (hooks.Count == 0)
        {
            logger.LogDebug("No post-step hooks registered.");
        }
        else
        {
            logger.LogDebug("Running post-step hooks...");
            foreach (var hook in hooks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Aborting post-step hooks due to cancellation request.");
                    return;
                }

                await hook.PostStep(summary, cancellationToken);
            }

            logger.LogDebug("Post-step hooks completed successfully.");
        }
    }
}
