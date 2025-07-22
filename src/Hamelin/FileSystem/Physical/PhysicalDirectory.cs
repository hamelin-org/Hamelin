namespace Hamelin.FileSystem.Physical;

internal class PhysicalDirectory(string path) : IDirectory
{
    public string Name { get; } = Path.GetFileName(path);
    public string AbsolutePath { get; } = Path.GetFullPath(path);
    public bool Exists => Directory.Exists(AbsolutePath);

    public void Create() => Directory.CreateDirectory(AbsolutePath);
    public void Delete() => Directory.Delete(AbsolutePath, true);

    public IFile GetFile(string name) => new PhysicalFile(Path.Combine(AbsolutePath, name));
    public IDirectory GetDirectory(string name) => new PhysicalDirectory(Path.Combine(AbsolutePath, name));

    public IEnumerable<IFile> GetFiles()
    {
        return Directory
            .EnumerateFiles(AbsolutePath)
            .Select(path => new PhysicalFile(path));
    }

    public IEnumerable<IDirectory> GetDirectories()
    {
        return Directory
            .EnumerateFiles(AbsolutePath)
            .Select(path => new PhysicalDirectory(path));
    }
}
