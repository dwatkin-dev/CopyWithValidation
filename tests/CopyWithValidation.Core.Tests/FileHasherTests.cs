using CopyWithValidation.Core.Tests.Fakes;
using CopyWithValidation.Core.Tests.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace CopyWithValidation.Core.Tests;

public class FileHasherTests
{
    [Fact]
    public async Task ComputeHashAsync_FileContainingAbc_ReturnsKnownSha256()
    {
        // Arrange
        var hasher = new FileHasher();
        using var files = new TempFiles();

        // Act
        string hash = await hasher.ComputeHashAsync(files.FileContainingAbc());

        // Assert
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
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

    [Fact]
    public async Task ComputeHashAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        using var files = new TempFiles();   // WriteSource not called, so no source file
        var hasher = new FileHasher();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => hasher.ComputeHashAsync(files.Source));
    }
}