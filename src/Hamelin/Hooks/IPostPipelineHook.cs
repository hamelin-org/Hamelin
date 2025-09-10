namespace Hamelin.Hooks;

/// <summary>
/// Allows for custom logic to be executed after the pipeline has completed.
/// </summary>
public interface IPostPipelineHook
{
    /// <summary>
    /// The method that will be called after the pipeline has completed execution.
    /// </summary>
    /// <param name="args">The arguments passed to the hook, including exit code and step results.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task PostPipeline(PostPipelineHookArgs args, CancellationToken cancellationToken = default);
}
