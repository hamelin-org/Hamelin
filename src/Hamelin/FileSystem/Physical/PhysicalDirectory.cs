namespace Hamelin.FileSystem.Physical;

internal class PhysicalDirectory(string path) : IDirectory
{
    public string Name { get; } = System.IO.Path.GetFileName(path);
    public string Path { get; } = path;
    public bool Exists => Directory.Exists(Path);

    public void Create() => Directory.CreateDirectory(Path);
    public void Delete() => Directory.Delete(Path, true);

    public IFile GetFile(string name) => new PhysicalFile(System.IO.Path.Combine(Path, name));
    public IDirectory GetDirectory(string name) => new PhysicalDirectory(System.IO.Path.Combine(Path, name));

    public IEnumerable<IFile> GetFiles()
    {
        return Directory
            .EnumerateFiles(Path)
            .Select(path => new PhysicalFile(path));
    }

    public IEnumerable<IDirectory> GetDirectories()
    {
        return Directory
            .EnumerateFiles(Path)
            .Select(path => new PhysicalDirectory(path));
    }
}
