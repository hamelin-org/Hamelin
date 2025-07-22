namespace Hamelin.FileSystem;

/// <summary>
/// Provides an abstraction for file system operations.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    IDirectory CurrentDirectory { get; }
}
