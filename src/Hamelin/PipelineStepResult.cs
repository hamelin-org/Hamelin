namespace Hamelin;

/// <summary>
/// Represents the result of a pipeline step execution.
/// </summary>
/// <param name="ExitCode">The exit code of the individual step.</param>
/// <param name="Continue">True if the pipeline should continue execution, otherwise false.</param>
/// <param name="Exception">The exception that was thrown by the pipeline step.</param>
public record PipelineStepResult(int ExitCode, bool Continue, Exception? Exception)
{
    /// <summary>
    /// Represents a successful pipeline step execution with no exceptions and continuation.
    /// </summary>
    public static readonly PipelineStepResult Successful = new(PipelineExitCodes.Success, true, null);

    /// <summary>
    /// Indicates that cancellation was requested, and the pipeline should stop execution.s
    /// </summary>
    public static readonly PipelineStepResult StoppedAfterCancel = new(PipelineExitCodes.StoppedAfterCancel, false, null);

    /// <summary>
    /// Indicates that the pipeline step resulted in an error, but the pipeline should continue execution.
    /// </summary>
    /// <param name="ex">The exception that was thrown.</param>
    /// <returns>The result.</returns>
    public static PipelineStepResult ContinuedAfterError(Exception ex) => new(PipelineExitCodes.ContinuedAfterError, true, ex);

    /// <summary>
    /// Indicates that the pipeline step resulted in an error, and the pipeline should stop execution.
    /// </summary>
    /// <param name="ex">The exception that was thrown.</param>
    /// <returns>The result.</returns>
    public static PipelineStepResult StoppedOnError(Exception ex) => new(PipelineExitCodes.StoppedOnError, false, ex);
}
