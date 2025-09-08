using Hamelin.FileSystem;
using Hamelin.FileSystem.Physical;
using Hamelin.Internal;
using Hamelin.Steps;
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
    private readonly PipelineApplicationOptions _options;

    /// <summary>
    /// Creates a new instance of the <see cref="PipelineApplicationBuilder"/> with the given options.
    /// </summary>
    /// <param name="options">The options to configure the pipeline application.</param>
    internal PipelineApplicationBuilder(PipelineApplicationOptions options)
    {
        _options = options;

        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>() {
            { "Logging:LogLevel:Microsoft.Hosting.Lifetime", nameof(LogLevel.Warning) }
        });

        // When left empty, the content root usually defaults to the current working directory.
        // Since Hamelin will usually be run from a project/repository directory, it won't be able to
        // find things like appsettings.json.
        string contentRoot = options.ContentRootPath ?? AppContext.BaseDirectory;

        _innerBuilder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = options.Args,
            ApplicationName = options.ApplicationName,
            EnvironmentName = options.EnvironmentName,
            ContentRootPath = contentRoot,
            Configuration = configuration,
        });

        // We have our own error handling in place for this.
        _innerBuilder.Services
            .Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

        _innerBuilder.Services
            .AddOptions<PipelineExecutionOptions>();

        Logging.AddPipelineConsoleFormatter();
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
        // We add services here to give the user a chance to supply their own before building the application.
        ApplyServices(Services);

        var host = _innerBuilder.Build();
        return new PipelineApplication(host);
    }

    /// <inheritdoc />
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    ) where TContainerBuilder : notnull => _innerBuilder.ConfigureContainer(factory, configure);

    private static void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.TryAddSingleton<IPipelineRunner, DefaultPipelineRunner>();
        services.TryAddSingleton<IPipelineStepRunner, DefaultPipelineStepRunner>();
        services.TryAddScoped<IFileSystem>(_ => new PhysicalFileSystem(System.Environment.CurrentDirectory));
        services.TryAddScoped<IPipelineState, DefaultPipelineState>();
        services.TryAddScoped<IPipelineContext, DefaultPipelineContext>();

        // Check if the user has supplied their own step provider, or register the default.
        bool hasProvider = services.Any(d => d.ServiceType == typeof(IPipelineStepProvider));
        if (!hasProvider)
        {
            services.TryAddSingleton<IPipelineStepCollection, PipelineStepCollection>();
            services.TryAddScoped<IPipelineStepProvider, PipelineStepProvider>();
        }

        // This is the service responsible for running the pipeline.
        services.AddHostedService<PipelineHost>();
    }
}
