using Hamelin.Internal;
using Hamelin.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Helpers;

internal class PipelineHostHelpers
{
    public static PipelineHost CreateHost(
        IPipelineStep[]? steps = null,
        Action<PipelineExecutionOptions>? configure = null,
        IPipelineContext? context = null,
        IHostApplicationLifetime? lifetime = null,
        ILogger<PipelineHost>? logger = null
    )
    {
        logger ??= Substitute.For<ILogger<PipelineHost>>();
        context ??= Substitute.For<IPipelineContext>();
        lifetime ??= Substitute.For<IHostApplicationLifetime>();

        var provider = Substitute.For<IPipelineStepProvider>();
        provider.GetSteps().Returns(steps ?? []);

        var services = new ServiceCollection()
            .AddSingleton(provider)
            .AddSingleton(context)
            .BuildServiceProvider();

        var scopeFactory = ServiceScopeHelpers.CreateScopeFactory(services);

        PipelineExecutionOptions pipelineExecutionOptions = new();
        configure?.Invoke(pipelineExecutionOptions);

        return new PipelineHost(logger, lifetime, scopeFactory, Options.Create(pipelineExecutionOptions));
    }
}
