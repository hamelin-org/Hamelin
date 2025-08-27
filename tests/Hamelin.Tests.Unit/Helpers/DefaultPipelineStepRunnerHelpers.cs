using Hamelin.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamelin.Tests.Unit.Helpers;

internal static class DefaultPipelineStepRunnerHelpers
{
    public static DefaultPipelineStepRunner CreateRunner(
        Action<PipelineExecutionOptions>? configure = null,
        ILogger<DefaultPipelineStepRunner>? logger = null
    )
    {
        logger ??= Substitute.For<ILogger<DefaultPipelineStepRunner>>();

        PipelineExecutionOptions pipelineExecutionOptions = new();
        configure?.Invoke(pipelineExecutionOptions);

        return new DefaultPipelineStepRunner(logger, Options.Create(pipelineExecutionOptions));
    }
}
