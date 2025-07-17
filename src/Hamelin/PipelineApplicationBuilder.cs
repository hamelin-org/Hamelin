using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        ApplyDefaultConfiguration(_innerBuilder.Environment, _innerBuilder.Configuration, options.Args);
        ApplyDefaultLogging(_innerBuilder.Logging);
        ApplyDefaultMetrics(_innerBuilder.Metrics);
        ApplyDefaultServices(_innerBuilder.Services);
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
    /// Builds the <see cref="PipelineApplication" />.
    /// </summary>
    /// <returns>The configured pipeline application.</returns>
    public PipelineApplication Build()
    {
        var host = _innerBuilder.Build();
        return new PipelineApplication(host);
    }

    /// <inheritdoc />
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    ) where TContainerBuilder : notnull => _innerBuilder.ConfigureContainer(factory, configure);

    private static void ApplyDefaultConfiguration(IHostEnvironment environment, ConfigurationManager configuration, string[]? args)
    {
        // Logic adapted from https://github.com/dotnet/runtime/blob/6149ca07d2202c2d0d518e10568c0d0dd3473576/src/libraries/Microsoft.Extensions.Hosting/src/HostingHostBuilderExtensions.cs#L229-L256
        bool reloadOnChange = configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
        configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange)
            .AddEnvironmentVariables()
            .AddCommandLine(args ?? []);
    }

    private static void ApplyDefaultLogging(ILoggingBuilder logging)
    {
        // TODO: Configure logging.
    }

    private static void ApplyDefaultMetrics(IMetricsBuilder metrics)
    {
        // TODO: Configure metrics.
    }

    private static void ApplyDefaultServices(IServiceCollection services)
    {
        // TODO: Configure services.

        // This is the service responsible for running the pipeline.
        services.AddHostedService<PipelineHost>();
    }
}
