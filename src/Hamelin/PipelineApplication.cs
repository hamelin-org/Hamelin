using Microsoft.Extensions.Hosting;

namespace Hamelin;

public class PipelineApplication : IHost
{
    private readonly IHost _host;

    internal PipelineApplication(IHost host)
    {
        _host = host;
    }

    public IServiceProvider Services => _host.Services;

    public Task StartAsync(CancellationToken cancellationToken) => _host.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _host.Dispose();
    }

    public static PipelineApplicationBuilder CreateBuilder() => new(new PipelineApplicationOptions());
    public static PipelineApplicationBuilder CreateBuilder(string[] args) => new(new PipelineApplicationOptions() { Args = args });
}
