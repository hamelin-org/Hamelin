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
        ILogger<PipelineHost>? logger = null,
        PipelineExecutionSummaryStore? summaryStore = null
    )
    {
        logger ??= Substitute.For<ILogger<PipelineHost>>();
        lifetime ??= Substitute.For<IHostApplicationLifetime>();
        summaryStore ??= new PipelineExecutionSummaryStore();

        PipelineExecutionOptions pipelineExecutionOptions = new();
        configure?.Invoke(pipelineExecutionOptions);

        var options = Options.Create(pipelineExecutionOptions);

        if (runner == null)
        {
            runner = Substitute.For<IPipelineRunner>();

            var context = Substitute.For<IPipelineContext>();
            context.ExitCode.Returns(0);

            var summary = new PipelineExecutionSummary(options, context, [], CancellationToken.None);

            runner
                .RunPipeline(Arg.Any<CancellationToken>())
                .Returns(summary);
        }

        return new PipelineHost(logger, options, lifetime, runner, summaryStore);
    }
}
