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

    [Fact] public void Name_ShouldBeFileName()
    {
        // Arrange
        string path = "FileSystem/Physical";

        // Act
        var dir = new PhysicalDirectory(path);

        // Assert
        dir.Name.ShouldBe("Physical");
    }
}
