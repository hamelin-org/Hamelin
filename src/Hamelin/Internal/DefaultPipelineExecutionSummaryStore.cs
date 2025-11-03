namespace Hamelin.Internal;
internal class DefaultPipelineExecutionSummaryStore : IPipelineExecutionSummaryStore
{
    private PipelineExecutionSummary? _summary;

    public void SetSummary(PipelineExecutionSummary summary)
    {
        if (_summary != null)
        {
            throw new InvalidOperationException("Pipeline execution summary has already been set.");
        }
        _summary = summary;
    }

    public PipelineExecutionSummary? GetAndClearSummary()
    {
        if (_summary == null)
        {
            return null;
        }
        var summary = _summary;
        _summary = null;
        return summary;
    }
}

