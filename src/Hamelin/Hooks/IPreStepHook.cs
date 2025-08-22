namespace Hamelin.Hooks;

/// <summary>
/// Allows for custom logic to be executed before each pipeline step.
/// </summary>
public interface IPreStepHook
{
    /// <summary>
    /// The method that will be called before each pipeline step.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task PreStep(CancellationToken cancellationToken = default);
}
