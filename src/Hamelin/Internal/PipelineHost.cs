using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Internal;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="logger">The logger for the pipeline host.</param>
/// <param name="options">Options to configure the pipeline execution</param>
/// <param name="lifetime">The application lifetime.</param>
/// <param name="runner">The service that will be used to run the pipeline.</param>
/// <param name="summaryStore">The store for the pipeline execution summary.</param>
internal class PipelineHost(
    ILogger<PipelineHost> logger,
    IOptions<PipelineExecutionOptions> options,
    IHostApplicationLifetime lifetime,
    IPipelineRunner runner,
    PipelineExecutionSummaryStore summaryStore
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var summary = await runner.RunPipeline(cancellationToken);
            summaryStore.Summary = summary;
            if (options.Value.SetEnvironmentExitCodeOnCompletion)
            {
                Environment.ExitCode = summary.ExitCode;
            }
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
}
