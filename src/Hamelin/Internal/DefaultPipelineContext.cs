using Microsoft.Extensions.FileProviders;

namespace Hamelin.Internal;

internal class DefaultPipelineContext(string currentDirectory) : IPipelineContext
{
    public IFileProvider FileSystem { get; } = new PhysicalFileProvider(currentDirectory);
    public string CurrentDirectory { get; } = currentDirectory;
}
