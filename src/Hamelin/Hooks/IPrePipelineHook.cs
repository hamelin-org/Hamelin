namespace Hamelin.Hooks;

/// <summary>
/// Allows for custom logic to be executed before the pipeline has started.
/// </summary>
public interface IPrePipelineHook
{
    /// <summary>
    /// The method that will be called before the pipeline has started.
    /// </summary>
    /// <param name="args">The arguments passed to the hook.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task PrePipeline(PrePipelineHookArgs args, CancellationToken cancellationToken = default);
}
