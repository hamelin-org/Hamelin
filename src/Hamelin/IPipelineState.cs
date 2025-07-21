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
    /// <param name="key">The key that the value was saved under, if any.</param>
    /// <returns>The value if one has been stored, or <c>default(T)</c> if not.</returns>
    T? Get<T>(string? key = null);

    /// <summary>
    /// Stores a value of type <typeparamref name="T"/> in the state. If a value of that type has already been stored, it will be replaced.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="key">An optional key under which the value should be saved.</param>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    void Set<T>(T value, string? key = null) where T : notnull;
}
