using Microsoft.Extensions.Logging;

namespace Hamelin.Build;

public class Esp : IExternalScopeProvider, IDisposable
{
    public void ForEachScope<TState>(Action<object?, TState> callback, TState state)
    {

    }

    public IDisposable Push(object? state)
    {
        return this;
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
