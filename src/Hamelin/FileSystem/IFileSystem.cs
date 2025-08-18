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

    /// <summary>
    /// Gets the file with the specified name on the file system.
    /// </summary>
    /// <param name="path">The name of the file to get.</param>
    /// <returns>The file object, regardless of whether the underlying file actually exists.</returns>
    IFile GetFile(string path);

    /// <summary>
    /// Gets the directory with the specified name on the file system.
    /// </summary>
    /// <param name="path">The name of the directory to get.</param>
    /// <returns>The directory object, regardless of whether the underlying directory actually exists.</returns>
    IDirectory GetDirectory(string path);
}
