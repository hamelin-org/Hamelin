using Microsoft.Extensions.Hosting;

namespace Hamelin;

/// <summary>
/// The hosted service that runs the pipeline.
/// </summary>
/// <param name="lifetime">The application lifetime.</param>
internal class PipelineHost(IHostApplicationLifetime lifetime) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement the logic to run the pipeline.
        Console.WriteLine("This is where the pipeline should get run.");

        // Exit the application gracefully now that the pipeline has run.
        lifetime.StopApplication();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
