using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;

namespace Hamelin;

/// <summary>
/// Represents the summary of a pipeline execution, including the exit code and results of each step.
/// </summary>
public class PipelineExecutionSummary
{
    /// <summary>
    /// Creates a new instance of <see cref="PipelineExecutionSummary"/>.
    /// </summary>
    public PipelineExecutionSummary(
        IOptions<PipelineExecutionOptions> options,
        IPipelineContext context,
        IEnumerable<StepExecutionSummary> steps,
        CancellationToken cancellationToken
    )
    {
        var stepList = steps.ToList().AsReadOnly();
        ExitCode = CalculateExitCode(options, context, stepList, cancellationToken);
        Steps = stepList;
    }

    /// <summary>
    /// Gets the exit code of the overall pipeline execution.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the results of each step in the pipeline execution.
    /// </summary>
    public IReadOnlyCollection<StepExecutionSummary> Steps { get; init; }

    private static int CalculateExitCode(IOptions<PipelineExecutionOptions> options, IPipelineContext context, ReadOnlyCollection<StepExecutionSummary> steps, CancellationToken cancellationToken)
    {
        int autoCode = steps.LastOrDefault()?.Result.ExitCode ?? PipelineExitCodes.Success;
        if (autoCode == PipelineExitCodes.Success && steps.Any(r => r.Result.ExitCode == PipelineExitCodes.ContinuedAfterError))
        {
            autoCode = PipelineExitCodes.ContinuedAfterError;
        }
        else if (steps.Count == 0 && cancellationToken.IsCancellationRequested)
        {
            autoCode = PipelineExitCodes.StoppedAfterCancel;
        }

        if (options.Value.EnableAutomaticExitCodes && autoCode != PipelineExitCodes.Success)
        {
            return autoCode;
        }

        return context.ExitCode ?? PipelineExitCodes.Success;
    }
}
