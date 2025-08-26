using Hamelin.Hooks;
using Hamelin.Internal;
using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Helpers;

internal static class DefaultPipelineRunnerHelpers
{
    public static DefaultPipelineRunner CreateRunner(
        IPipelineStep[]? steps = null,
        IPrePipelineHook[]? prePipelineHooks = null,
        IPostPipelineHook[]? postPipelineHooks = null,
        IPipelineStepRunner? stepRunner = null,
        Action<PipelineExecutionOptions>? configure = null,
        IPipelineContext? context = null,
        ILogger<DefaultPipelineRunner>? logger = null
    )
    {
        logger ??= Substitute.For<ILogger<DefaultPipelineRunner>>();
        context ??= Substitute.For<IPipelineContext>();
        stepRunner ??= Substitute.For<IPipelineStepRunner>();

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

        return new DefaultPipelineRunner(logger, Options.Create(pipelineExecutionOptions), scopeFactory, stepRunner);
    }
}
