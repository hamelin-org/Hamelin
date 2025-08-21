using Hamelin.FileSystem.Physical;

namespace Hamelin.Tests.Unit.FileSystem.Physical;

public class PhysicalFileTests
{
    [Fact]
    public void Exists_FileDoesNotExist_ShouldBeFalse()
    {
        // Arrange
        string path = "ThisFileDoesNotExist.txt";

        // Act
        var file = new PhysicalFile(path);

        // Assert
        file.Exists.ShouldBeFalse();
    }

    [Fact]
    public void Exists_FileExists_ShouldBeTrue()
    {
        // Arrange
        string path = "FileSystem/Physical/TestFile.txt";

        // Act
        var file = new PhysicalFile(path);

        // Assert
        file.Exists.ShouldBeTrue();
    }

    [Fact]
    public void Name_ShouldBeFileName()
    {
        // Arrange
        string path = "FileSystem/Physical/TestFile.txt";

        // Act
        var file = new PhysicalFile(path);

        // Assert
        file.Name.ShouldBe("TestFile.txt");
    }

    [Fact]
    public void NameWithoutExtension_ShouldBeFileNameWithoutExtension()
    {
        // Arrange
        string path = "FileSystem/Physical/TestFile.txt";

        // Act
        var file = new PhysicalFile(path);

        // Assert
        file.NameWithoutExtension.ShouldBe("TestFile");
    }

    [Fact]
    public void Extension_ShouldBeFileExtension()
    {
        // Arrange
        string path = "FileSystem/Physical/TestFile.txt";

        // Act
        var file = new PhysicalFile(path);

        // Assert
        file.Extension.ShouldBe(".txt");
    }

    [Fact]
    public async Task OpenRead_ExistingFile_ShouldAllowRead()
    {
        // Arrange
        string path = "FileSystem/Physical/TestFile.txt";
        var file = new PhysicalFile(path);

        // Act
        await using var stream = file.OpenRead();
        using var sr = new StreamReader(stream);
        string content = await sr.ReadToEndAsync(TestContext.Current.CancellationToken);

        // Assert
        content.Trim().ShouldBe("This file does exist.");
    }

    [Fact]
    public void Delete_ExistingFile_ShouldAllowRead()
    {
        // Arrange
        string path = Path.GetTempFileName();
        var file = new PhysicalFile(path);
        file.Exists.ShouldBe(true);

        // Act
        file.Delete();

        // Assert
        file.Exists.ShouldBe(false);
    }
}
