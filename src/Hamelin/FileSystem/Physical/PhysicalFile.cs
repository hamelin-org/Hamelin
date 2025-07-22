namespace Hamelin.FileSystem.Physical;

internal class PhysicalFile(string path) : IFile
{
    public string Name { get; } = System.IO.Path.GetFileName(path);
    public string Path { get; } = path;
    public bool Exists => File.Exists(Path);
    public Stream OpenRead() => File.OpenRead(Path);
    public Stream OpenWrite() => File.OpenWrite(Path);
}
