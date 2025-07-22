namespace Hamelin.FileSystem.Physical;

internal class PhysicalFileSystem(string path) : IFileSystem
{
    public IDirectory CurrentDirectory { get; } = new PhysicalDirectory(path);
}
