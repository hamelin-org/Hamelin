using Hamelin.FileSystem;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(IFileSystem fileSystem, IPipelineState state, IHostEnvironment env) : IPipelineContext
{
    public IFileSystem FileSystem { get; } = fileSystem;
    public IPipelineState State { get; } = state;
    public string CurrentDirectory { get; } = env.ContentRootPath;
}
