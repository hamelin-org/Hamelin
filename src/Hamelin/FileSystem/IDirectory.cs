namespace Hamelin.FileSystem;

/// <summary>
/// Represents a directory in the file system.
/// </summary>
public interface IDirectory
{
    /// <summary>
    /// Gets the name of the directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full path to the directory.
    /// </summary>
    string AbsolutePath { get; }

    /// <summary>
    /// Gets a value indicating whether this directory exists in the file system.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Deletes this directory from the file system if it exists.
    /// </summary>
    void Delete();

    /// <summary>
    /// Gets the file with the specified name within this directory.
    /// </summary>
    /// <param name="name">The name of the file to get.</param>
    /// <returns>The file object, regardless of whether the underlying file actually exists.</returns>
    IFile GetFile(string name);

    /// <summary>
    /// Gets the directory with the specified name within this directory.
    /// </summary>
    /// <param name="name">The name of the directory to get.</param>
    /// <returns>The directory object, regardless of whether the underlying directory actually exists.</returns>
    IDirectory GetDirectory(string name);

    /// <summary>
    /// Gets the files contained in this directory.
    /// </summary>
    /// <param name="searchPattern">The search pattern to match file names against. Supports globbing.</param>
    /// <returns>The files in this directory.</returns>
    IEnumerable<IFile> GetFiles(string searchPattern = "*");

    /// <summary>
    /// Gets the subdirectories contained in this directory.
    /// </summary>
    /// <returns>The subdirectories in this directory.</returns>
    IEnumerable<IDirectory> GetDirectories();
}
