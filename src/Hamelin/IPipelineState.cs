namespace Hamelin;

/// <summary>
/// Provides a mechanism to store and retrieve state during the execution of a pipeline.
/// </summary>
public interface IPipelineState
{
    /// <summary>
    /// Gets a value of type <typeparamref name="T"/> from the state, if one has been stored previously.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <returns>The value if one has been stored, or <c>default(T)</c> if not.</returns>
    T? Get<T>();

    /// <summary>
    /// Stores a value of type <typeparamref name="T"/> in the state. If a value of that type has already been stored, it will be replaced.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    void Set<T>(T value) where T : notnull;
}
