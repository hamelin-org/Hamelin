using Hamelin.FileSystem.Physical;

namespace Hamelin.Tests.Unit.FileSystem.Physical;

public class PhysicalFileSystemTests
{
    [Fact]
    public void CurrentDirectory_IsCurrentDirectory()
    {
        // Arrange
        string directory = Directory.GetCurrentDirectory();

        // Act
        var fileSystem = new PhysicalFileSystem(directory);

        // Assert
        fileSystem.CurrentDirectory.AbsolutePath.ShouldBe(directory);
    }

    [Fact]
    public void RootDirectory_IsRootDirectory()
    {
        // Arrange
        string directory = Directory.GetCurrentDirectory();
        string? root = Path.GetPathRoot(directory);

        // Act
        var fileSystem = new PhysicalFileSystem(directory);

        // Assert
        fileSystem.RootDirectory.AbsolutePath.ShouldBe(root);
    }
}
