namespace Hamelin.FileSystem;

/// <summary>
/// Represents a file in the file system.
/// </summary>
public interface IFile
{
    /// <summary>
    /// Gets the name of the file, including the extension.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full path to the file.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets a value indicating whether this directory exists in the file system.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Opens the file for reading.
    /// </summary>
    /// <returns>A stream that can be read from.</returns>
    Stream OpenRead();

    /// <summary>
    /// Opens the file for writing. If the file does not exist, it will be created.
    /// </summary>
    /// <returns>A stream that can be written to.</returns>
    Stream OpenWrite();
}
