namespace Hamelin.Hooks;

/// <summary>
/// Allows for custom logic to be executed after each pipeline step.
/// </summary>
public interface IPostStepHook
{
    /// <summary>
    /// The method that will be called after each pipeline step.
    /// </summary>
    /// <param name="args">The arguments passed to the hook, including exit code and step results.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task PostStep(PostStepHookArgs args, CancellationToken cancellationToken = default);
}
