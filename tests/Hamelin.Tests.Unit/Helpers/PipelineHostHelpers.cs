using Hamelin.Hooks;
using Hamelin.Internal;
using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Helpers;

internal static class PipelineHostHelpers
{
    public static PipelineHost CreateHost(
        IPipelineStep[]? steps = null,
        IPrePipelineHook[]? prePipelineHooks = null,
        IPostPipelineHook[]? postPipelineHooks = null,
        Action<PipelineExecutionOptions>? configure = null,
        IPipelineContext? context = null,
        IHostApplicationLifetime? lifetime = null,
        ILogger<PipelineHost>? logger = null
    )
    {
        logger ??= Substitute.For<ILogger<PipelineHost>>();
        context ??= Substitute.For<IPipelineContext>();
        lifetime ??= Substitute.For<IHostApplicationLifetime>();

        var stepProvider = Substitute.For<IPipelineStepProvider>();
        stepProvider.GetSteps().Returns(steps ?? []);

        var services = new ServiceCollection()
            .AddSingleton(stepProvider)
            .AddSingleton(context);

        foreach (var hook in prePipelineHooks ?? [])
        {
            services.AddScoped<IPrePipelineHook>(_ => hook);
        }
        foreach (var hook in postPipelineHooks ?? [])
        {
            services.AddScoped<IPostPipelineHook>(_ => hook);
        }

        var provider = services.BuildServiceProvider();
        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(provider);

        PipelineExecutionOptions pipelineExecutionOptions = new();
        configure?.Invoke(pipelineExecutionOptions);

        return new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));
    }
}
