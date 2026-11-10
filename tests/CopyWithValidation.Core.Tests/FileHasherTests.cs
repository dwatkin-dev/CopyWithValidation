using CopyWithValidation.Core.Tests.Helpers;
using System.Text;

namespace CopyWithValidation.Core.Tests;

public class FileHasherTests
{
    [Fact]
    public void ComputeHash_EmptyStream_ReturnsKnownSha256()
    {
        // Arrange
        var hasher = new FileHasher();
        using var stream = new MemoryStream();

        // Act
        string hash = hasher.ComputeHash(stream);

        // Assert
        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
    }

    [Fact]
    public void ComputeHash_Abc_ReturnsKnownSha256()
    {
        // Arrange
        var hasher = new FileHasher();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));

        // Act
        string hash = hasher.ComputeHash(stream);

        // Assert
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public void ComputeHash_FileContainingAbc_ReturnsKnownHash()
    {
        // Arrange
        var hasher = new FileHasher();
        var files = new TempFiles();

        // Act
        string hash = hasher.ComputeHash(files.FileContainingAbc());

        // Assert
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public void ComputeHash_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var hasher = new FileHasher();
        var files = new TempFiles();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => hasher.ComputeHash(files.Source));
    }
}
