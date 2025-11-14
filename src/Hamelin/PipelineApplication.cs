using Hamelin.Internal;
using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hamelin;

/// <summary>
/// Represents a CI/CD pipeline application that can be run.
/// </summary>
public class PipelineApplication : IHost
{
    private bool _hasRun = false;
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
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_hasRun)
        {
            throw new InvalidOperationException("The pipeline application can only be started once.");
        }
        _hasRun = true;
        return _host.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);

    /// <summary>
    /// Registers a step with the pipeline that will be run when the application is executed.
    /// Steps are resolved from the service provider and executed in the order they were added.
    /// </summary>
    /// <typeparam name="TStep">The type of the step to add. It must implement <see cref="IPipelineStep"/>.</typeparam>
    /// <returns>The current <see cref="PipelineApplication"/> instance.</returns>
    public PipelineApplication UseStep<TStep>() where TStep : class, IPipelineStep => UseStep(typeof(TStep));

    /// <summary>
    /// Registers a step with the pipeline that will be run when the application is executed.
    /// Steps are resolved from the service provider and executed in the order they were added.
    /// </summary>
    /// <param name="step">The type of the step to add. It must implement <see cref="IPipelineStep"/>.</param>
    /// <returns>The current <see cref="PipelineApplication"/> instance.</returns>
    public PipelineApplication UseStep(Type step)
    {
        var collector = _host.Services.GetService<IPipelineStepCollection>();
        if (collector == null)
        {
            throw new InvalidOperationException("This method of step registration is not supported when a custom IPipelineStepProvider has been configured.");
        }
        collector.AddStep(step);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _host.Dispose();
    }

    /// <summary>
    /// Runs the pipeline and returns the exit code from the execution.
    /// </summary>
    /// <returns>The exit code from the pipeline execution.</returns>
    public int RunWithExitCode()
    {
        return RunWithExitCodeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs the pipeline and returns the exit code from the execution.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The exit code from the pipeline execution.</returns>
    public async Task<int> RunWithExitCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            await this.WaitForShutdownAsync(cancellationToken).ConfigureAwait(false);

            // Retrieve the exit code from the pipeline execution summary.
            var store = Services.GetRequiredService<PipelineExecutionSummaryStore>();
            return store.Summary?.ExitCode ?? PipelineExitCodes.MissingSummary;
        }
        finally
        {
            Dispose();
        }
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
