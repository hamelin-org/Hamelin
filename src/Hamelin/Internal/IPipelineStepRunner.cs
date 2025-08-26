using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Internal;

internal interface IPipelineStepRunner
{
    Task<PipelineStepResult> RunStep(AsyncServiceScope scope, IPipelineStep step, CancellationToken cancellationToken = default);
}
