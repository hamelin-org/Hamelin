namespace Hamelin.Internal;

internal interface IPipelineRunner
{
    Task<PipelineExecutionSummary> RunPipeline(CancellationToken cancellationToken);
}
