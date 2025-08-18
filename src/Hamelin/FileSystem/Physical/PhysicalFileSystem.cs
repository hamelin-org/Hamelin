namespace Hamelin.FileSystem.Physical;

internal class PhysicalFileSystem(string path) : IFileSystem
{
    public IDirectory CurrentDirectory { get; } = new PhysicalDirectory(path);

    public IFile GetFile(string path)
    {
        return Path.IsPathRooted(path)
            ? new PhysicalFile(path)
            : throw new ArgumentException("Path must be absolute.");
    }

    public IDirectory GetDirectory(string path)
    {
        return Path.IsPathRooted(path)
            ? new PhysicalDirectory(path)
            : throw new ArgumentException("Path must be absolute.");
    }
}
