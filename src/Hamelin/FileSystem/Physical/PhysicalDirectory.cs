using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Hamelin.FileSystem.Physical;

[DebuggerDisplay("{Name} ({AbsolutePath})")]
internal class PhysicalDirectory(string path) : IDirectory
{
    public string Name { get; } = Path.GetFileName(path);
    public string AbsolutePath { get; } = Path.GetFullPath(path);
    public bool Exists => Directory.Exists(AbsolutePath);

    public void Delete()
    {
        if (Exists)
        {
            Directory.Delete(AbsolutePath, true);
        }
    }

    public IFile GetFile(string name) => new PhysicalFile(Path.Combine(AbsolutePath, name));
    public IDirectory GetDirectory(string name) => new PhysicalDirectory(Path.Combine(AbsolutePath, name));

    public IEnumerable<IFile> GetFiles(string searchPattern = "*")
    {
        var matcher = new Matcher();
        matcher.AddInclude(searchPattern);

        var paths = matcher.GetResultsInFullPath(AbsolutePath);
        return paths.Select(path => new PhysicalFile(path));
    }

    public IEnumerable<IDirectory> GetDirectories()
    {
        return Directory
            .EnumerateDirectories(AbsolutePath)
            .Select(path => new PhysicalDirectory(path));
    }

    public override string ToString() => AbsolutePath;
}
