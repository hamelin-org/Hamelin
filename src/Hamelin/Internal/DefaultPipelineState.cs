namespace Hamelin.Internal;

internal class DefaultPipelineState : IPipelineState
{
    private readonly Dictionary<(Type, string), object> _state = new();

    public T? Get<T>(string? key = null)
    {
        if (_state.TryGetValue((typeof(T), key ?? ""), out object? value))
        {
            return (T)value;
        }

        return default;
    }

    public void Set<T>(T value, string? key = null) where T : notnull
    {
        _state[(typeof(T), key ?? "")] = value;
    }
}
