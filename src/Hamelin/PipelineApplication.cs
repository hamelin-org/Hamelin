using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hamelin;

/// <summary>
/// Represents a CI/CD pipeline application that can be run.
/// </summary>
public class PipelineApplication : IHost
{
    private readonly IHost _host;

    /// <summary>
    /// Creates a new instance of the <see cref="PipelineApplication"/> class with the specified host.
    /// </summary>
    /// <param name="host">The host that will run the pipeline.</param>
    internal PipelineApplication(IHost host)
    {
        _host = host;
    }

    /// <inheritdoc />
    public IServiceProvider Services => _host.Services;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => _host.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);

    /// <summary>
    /// Adds a step to the pipeline that will be run when the application is executed.
    /// Steps are resolved from the service provider and executed in the order they were added.
    /// </summary>
    /// <typeparam name="TStep">The type of the step to add. It must implement <see cref="IPipelineStep"/>.</typeparam>
    /// <returns>The current <see cref="PipelineApplication"/> instance.</returns>
    public PipelineApplication RunStep<TStep>() where TStep : class, IPipelineStep
    {
        var stepRepo = _host.Services.GetRequiredService<PipelineStepCollection>();
        stepRepo.AddStep<TStep>();
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _host.Dispose();
    }

    /// <summary>
    /// Creates a new <see cref="PipelineApplicationBuilder"/> with default settings
    /// that can be used to configure a CI/CD pipeline.
    /// </summary>
    /// <returns>The created builder.</returns>
    public static PipelineApplicationBuilder CreateBuilder() => new(new PipelineApplicationOptions());

    /// <summary>
    /// Creates a new <see cref="PipelineApplicationBuilder"/> with default settings
    /// that can be used to configure a CI/CD pipeline.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>The created builder.</returns>
    public static PipelineApplicationBuilder CreateBuilder(string[] args) => new(new PipelineApplicationOptions() { Args = args });

    /// <summary>
    /// Creates a new <see cref="PipelineApplicationBuilder"/> with default settings
    /// that can be used to configure a CI/CD pipeline.
    /// </summary>
    /// <param name="options">The options to pass the pipeline builder.</param>
    /// <returns>The created builder.</returns>
    public static PipelineApplicationBuilder CreateBuilder(PipelineApplicationOptions options) => new(options);
}
