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
    public void GetDirectory_AbsolutePath_Succeeds()
    {
        // Arrange
        string directory = Directory.GetCurrentDirectory();
        var fileSystem = new PhysicalFileSystem(directory);

        // Act
        var currentDirectory = fileSystem.GetDirectory(directory);

        // Assert
        currentDirectory.AbsolutePath.ShouldBe(directory);
    }

    [Fact]
    public void GetDirectory_RelativePath_Throws()
    {
        // Arrange
        string directory = Directory.GetCurrentDirectory();
        var fileSystem = new PhysicalFileSystem(directory);

        // Act
        var act = () => fileSystem.GetDirectory("./relative-path");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void GetFile_RelativePath_Throws()
    {
        // Arrange
        string directory = Directory.GetCurrentDirectory();
        var fileSystem = new PhysicalFileSystem(directory);

        // Act
        var act = () => fileSystem.GetFile("./relative-file.txt");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void GetFile_AbsolutePath_Succeeds()
    {
        // Arrange
        string file = Directory.GetCurrentDirectory() + "/TestFile.txt";
        var fileSystem = new PhysicalFileSystem(file);

        // Act
        var currentFile = fileSystem.GetFile(file);

        // Assert
        currentFile.AbsolutePath.ShouldBe(file);
    }
}
