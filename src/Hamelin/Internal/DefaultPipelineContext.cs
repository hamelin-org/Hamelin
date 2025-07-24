using Hamelin.FileSystem;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(IFileSystem fileSystem, IPipelineState state) : IPipelineContext
{
    public IFileSystem FileSystem { get; } = fileSystem;
    public IPipelineState State { get; } = state;
    public string CurrentDirectory => Environment.CurrentDirectory;
}
