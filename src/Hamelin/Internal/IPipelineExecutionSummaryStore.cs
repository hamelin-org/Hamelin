namespace Hamelin.Internal;

internal interface IPipelineExecutionSummaryStore
{
    public void SetSummary(PipelineExecutionSummary summary);
    public PipelineExecutionSummary? GetAndClearSummary();
}
