using CopyWithValidation.Core.Tests.Fakes;
using CopyWithValidation.Core.Tests.Helpers;
using System.Security.Cryptography;
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
        using var files = new TempFiles();

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
        using var files = new TempFiles();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => hasher.ComputeHash(files.Source));
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task ComputeHashAsync_AnySize_MatchesSha256OfContents(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var hasher = new FileHasher();
        byte[] data = files.WriteSource(size);
        string expected = Convert.ToHexStringLower(SHA256.HashData(data));

        // Act
        string hash = await hasher.ComputeHashAsync(files.Source);

        // Assert
        Assert.Equal(expected, hash);
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task ComputeHashAsync_AnySize_LastProgressReportIs100(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var hasher = new FileHasher();
        var progress = new RecordingProgress<double>();
        files.WriteSource(size);

        // Act
        await hasher.ComputeHashAsync(files.Source, progress);

        // Assert
        Assert.Equal(100.0, progress.Values.Last());
    }

    [Fact]
    public async Task ComputeHashAsync_MultiBufferFile_ReportsIncreasingProgress()
    {
        // Arrange
        using var files = new TempFiles();
        var hasher = new FileHasher();
        var progress = new RecordingProgress<double>();
        files.WriteSource(10 * 1024 * 1024);   // 10 MB

        // Act
        await hasher.ComputeHashAsync(files.Source, progress);

        // Assert
        Assert.True(progress.Values.Count > 1);
        Assert.Equal(progress.Values.Order(), progress.Values);   // LINQ: sorted copy
        Assert.Equal(100.0, progress.Values.Last());
    }
}