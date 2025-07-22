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
    string Path { get; }

    /// <summary>
    /// Gets a value indicating whether this directory exists in the file system.
    /// </summary>
    bool Exists { get; }

    // Actions

    /// <summary>
    /// Creates this directory in the file system.
    /// </summary>
    void Create();

    /// <summary>
    /// Deletes this directory and its contents.
    /// </summary>
    void Delete();

    // Navigation

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
    /// <param name="searchPattern">The search pattern to match files against. Supports globbing.</param>
    /// <param name="recursive">If true, searches subdirectories recursively.</param>
    /// <returns>The files in this directory.</returns>
    IEnumerable<IFile> GetFiles(string searchPattern = "*.*", bool recursive = false);

    /// <summary>
    /// Gets the subdirectories contained in this directory.
    /// </summary>
    /// <param name="searchPattern">The search pattern to match directories against. Supports globbing.</param>
    /// <param name="recursive">If true, searches subdirectories recursively.</param>
    /// <returns>The discovered directories.</returns>
    IEnumerable<IDirectory> GetDirectories(string searchPattern = "*", bool recursive = false);
}
