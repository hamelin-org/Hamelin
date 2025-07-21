using Microsoft.Extensions.FileProviders;

namespace Hamelin;

/// <summary>
/// A service that can be injected to provide context for the current pipeline execution.
/// </summary>
public interface IPipelineContext
{
    /// <summary>
    /// Gets an abstraction over the file system that can be used to access files and directories.
    /// </summary>
    public IFileProvider FileSystem { get; }
}
