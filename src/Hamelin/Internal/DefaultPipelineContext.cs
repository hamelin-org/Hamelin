using Microsoft.Extensions.FileProviders;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(IFileProvider fileSystem) : IPipelineContext
{
    public IFileProvider FileSystem { get; } = fileSystem;
}
