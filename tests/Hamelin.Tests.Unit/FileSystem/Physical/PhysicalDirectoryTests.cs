using Hamelin.FileSystem.Physical;

namespace Hamelin.Tests.Unit.FileSystem.Physical;

public class PhysicalDirectoryTests
{
    [Fact]
    public void Exists_DirectoryDoesNotExist_ShouldBeFalse()
    {
        // Arrange
        string path = "./foobar";

        // Act
        var dir = new PhysicalDirectory(path);

        // Assert
        dir.Exists.ShouldBeFalse();
    }

    [Fact]
    public void Exists_DirectoryExists_ShouldBeTrue()
    {
        // Arrange
        string path = "FileSystem/Physical";

        // Act
        var dir = new PhysicalDirectory(path);

        // Assert
        dir.Exists.ShouldBeTrue();
    }

    [Fact]
    public void Name_ShouldBeFileName()
    {
        // Arrange
        string path = "FileSystem/Physical";

        // Act
        var dir = new PhysicalDirectory(path);

        // Assert
        dir.Name.ShouldBe("Physical");
    }

    [Fact]
    public void GetDirectory_ExistingDirectory_GetsDirectory()
    {
        // Arrange"
        string path = "./";
        string directory = "FileSystem";
        var dir = new PhysicalDirectory(path);

        // Act
        var subDir = dir.GetDirectory(directory);

        // Assert
        subDir.Exists.ShouldBeTrue();
        subDir.Name.ShouldBe("FileSystem");
    }

    [Fact]
    public void GetDirectory_MissingDirectory_StillGetsDirectory()
    {
        // Arrange"
        string path = "./";
        string directory = "DoesNotExist";
        var dir = new PhysicalDirectory(path);

        // Act
        var subDir = dir.GetDirectory(directory);

        // Assert
        subDir.Exists.ShouldBeFalse();
        subDir.Name.ShouldBe("DoesNotExist");
    }

    [Fact]
    public void GetDirectories_ShouldContainKnownDirectory()
    {
        // Arrange
        string path = "./";
        var dir = new PhysicalDirectory(path);

        // Act
        var directories = dir.GetDirectories();

        // Assert
        directories.ShouldContain(d => d.Name == "FileSystem");
    }

    [Fact]
    public void GetFiles_AllFiles_ShouldContainKnownFile()
    {
        // Arrange
        string path = "./";
        var dir = new PhysicalDirectory(path);

        // Act
        var files = dir.GetFiles();

        // Assert
        files.ShouldContain(d => d.Name == "Hamelin.dll");
    }

    [Fact]
    public void GetFiles_AllFiles_ShouldNotContainFilesInSubdirectories()
    {
        // Arrange
        string path = "./";
        var dir = new PhysicalDirectory(path);

        // Act
        var files = dir.GetFiles();

        // Assert
        files.ShouldNotContain(d => d.Name == "TestFile.txt");
    }

    [Fact]
    public void GetFiles_Search_ShouldReturnMatchingFile()
    {
        // Arrange
        string path = "./";
        var dir = new PhysicalDirectory(path);

        // Act
        var files = dir.GetFiles("**/*.txt");

        // Assert
        files.ShouldContain(d => d.Name == "TestFile.txt");
    }
}
