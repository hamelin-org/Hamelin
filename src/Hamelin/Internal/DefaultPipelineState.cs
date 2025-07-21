namespace Hamelin.Internal;

internal class DefaultPipelineState : IPipelineState
{
    private readonly Dictionary<Type, object> _state = new();

    public T? Get<T>()
    {
        if (_state.TryGetValue(typeof(T), out object? value))
        {
            return (T)value;
        }
        return default;
    }

    public void Set<T>(T value) where T : notnull
    {
        _state[typeof(T)] = value;
    }
}
