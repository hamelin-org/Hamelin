namespace Hamelin.FileSystem.Physical;

internal class PhysicalFileSystem(string path) : IFileSystem
{
    public IDirectory CurrentDirectory { get; } = new PhysicalDirectory(path);

    public IDirectory RootDirectory { get; } = ResolveRoot(path);

    private static PhysicalDirectory ResolveRoot(string path)
    {
        string absolutePath = Path.GetFullPath(path);
        string root = Path.GetPathRoot(absolutePath)
                      ?? throw new ArgumentException("Path must have a root.", nameof(path));
        return new PhysicalDirectory(root);
    }
}
