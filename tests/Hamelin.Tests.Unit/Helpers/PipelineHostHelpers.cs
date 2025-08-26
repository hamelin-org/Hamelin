using Hamelin.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Helpers;

internal static class PipelineHostHelpers
{
    public static PipelineHost CreateHost(
        IPipelineRunner? runner = null,
        Action<PipelineExecutionOptions>? configure = null,
        IHostApplicationLifetime? lifetime = null,
        ILogger<PipelineHost>? logger = null
    )
    {
        logger ??= Substitute.For<ILogger<PipelineHost>>();
        lifetime ??= Substitute.For<IHostApplicationLifetime>();
        runner ??= Substitute.For<IPipelineRunner>();

        PipelineExecutionOptions pipelineExecutionOptions = new();
        configure?.Invoke(pipelineExecutionOptions);

        return new PipelineHost(logger, Options.Create(pipelineExecutionOptions), lifetime, runner);
    }
}
