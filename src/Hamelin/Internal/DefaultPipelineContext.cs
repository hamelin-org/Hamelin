using Microsoft.Extensions.FileProviders;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(IFileProvider fileSystem, IPipelineState state) : IPipelineContext
{
    public IFileProvider FileSystem { get; } = fileSystem;
    public IPipelineState State { get; } = state;
}
