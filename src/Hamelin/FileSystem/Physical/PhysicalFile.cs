using System.Diagnostics;

namespace Hamelin.FileSystem.Physical;

[DebuggerDisplay("{Name} ({AbsolutePath})")]
internal class PhysicalFile(string path) : IFile
{
    public string Name { get; } = Path.GetFileName(path);
    public string AbsolutePath { get; } = Path.GetFullPath(path);
    public bool Exists => File.Exists(AbsolutePath);
    public Stream OpenRead() => File.OpenRead(AbsolutePath);
    public Stream OpenWrite() => File.OpenWrite(AbsolutePath);

    public override string ToString() => AbsolutePath;
}
