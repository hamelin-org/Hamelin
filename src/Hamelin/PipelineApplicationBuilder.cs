using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hamelin;

public class PipelineApplicationBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _innerBuilder;

    internal PipelineApplicationBuilder(PipelineApplicationOptions options)
    {
        var configuration = new ConfigurationManager();
        _innerBuilder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = options.Args,
            ApplicationName = options.ApplicationName,
            EnvironmentName = options.EnvironmentName,
            ContentRootPath = options.ContentRootPath,
            Configuration = configuration,
        });

        ApplyDefaultConfiguration(_innerBuilder.Environment, configuration, options.Args);
        ApplyDefaultLogging(_innerBuilder.Logging);
        ApplyDefaultMetrics(_innerBuilder.Metrics);
        ApplyDefaultServices(_innerBuilder.Services);
    }

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_innerBuilder).Properties;
    public IConfigurationManager Configuration => _innerBuilder.Configuration;
    public IHostEnvironment Environment => _innerBuilder.Environment;
    public ILoggingBuilder Logging => _innerBuilder.Logging;
    public IMetricsBuilder Metrics => _innerBuilder.Metrics;
    public IServiceCollection Services => _innerBuilder.Services;

    public PipelineApplication Build()
    {
        var host = _innerBuilder.Build();
        return new PipelineApplication(host);
    }

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
        // At least one `IHostedService` should be registered to run the application.
    }
}
