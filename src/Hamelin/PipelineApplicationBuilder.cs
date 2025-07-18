using Hamelin.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hamelin;

/// <summary>
/// Provides functionality to configure and build a CI/CD pipeline application.
/// </summary>
public class PipelineApplicationBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _innerBuilder;

    /// <summary>
    /// Creates a new instance of the <see cref="PipelineApplicationBuilder"/> with the given options.
    /// </summary>
    /// <param name="options">The options to configure the pipeline application.</param>
    internal PipelineApplicationBuilder(PipelineApplicationOptions options)
    {
        _innerBuilder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = options.Args,
            ApplicationName = options.ApplicationName,
            EnvironmentName = options.EnvironmentName,
            ContentRootPath = options.ContentRootPath,
            Configuration = new ConfigurationManager(),
        });

        ApplyMandatoryServices(_innerBuilder.Services);
    }

    /// <inheritdoc />
    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_innerBuilder).Properties;

    /// <inheritdoc />
    public IConfigurationManager Configuration => _innerBuilder.Configuration;

    /// <inheritdoc />
    public IHostEnvironment Environment => _innerBuilder.Environment;

    /// <inheritdoc />
    public ILoggingBuilder Logging => _innerBuilder.Logging;

    /// <inheritdoc />
    public IMetricsBuilder Metrics => _innerBuilder.Metrics;

    /// <inheritdoc />
    public IServiceCollection Services => _innerBuilder.Services;

    /// <summary>
    /// Adds a step to the pipeline that will be run when the application is executed.
    /// </summary>
    /// <typeparam name="TStep"></typeparam>
    /// <returns></returns>
    public PipelineApplicationBuilder AddStep<TStep>() where TStep : class, IPipelineStep
    {
        Services.AddStep<TStep>();
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PipelineApplication" />.
    /// </summary>
    /// <returns>The configured pipeline application.</returns>
    public PipelineApplication Build()
    {
        // We add optional services here to give the user a chance to supply their own before building the application.
        ApplyOverridableServices(Services);

        var host = _innerBuilder.Build();
        return new PipelineApplication(host);
    }

    /// <inheritdoc />
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    ) where TContainerBuilder : notnull => _innerBuilder.ConfigureContainer(factory, configure);

    private static void ApplyOverridableServices(IServiceCollection services)
    {
        // Check if the user has supplied their own step provider, or register the default.
        if (services.Any(d => d.ServiceType == typeof(IPipelineStepProvider)))
        {
            services.TryAddSingleton<PipelineStepCollection>();
            services.AddSingleton<IPipelineStepCollector>(sp => sp.GetRequiredService<PipelineStepCollection>());
            services.AddSingleton<IPipelineStepProvider>(sp => sp.GetRequiredService<PipelineStepCollection>());
        }
    }

    private static void ApplyMandatoryServices(IServiceCollection services)
    {
        // This is the service responsible for running the pipeline.
        services.AddHostedService<PipelineHost>();
    }
}
