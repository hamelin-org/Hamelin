using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(IFileProvider fileSystem, IPipelineState state, IHostEnvironment env) : IPipelineContext
{
    public IFileProvider FileSystem { get; } = fileSystem;
    public IPipelineState State { get; } = state;
    public string CurrentDirectory { get; } = env.ContentRootPath;
}
