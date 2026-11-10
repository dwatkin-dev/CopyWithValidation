using CopyWithValidation.Core.Tests.Fakes;
using CopyWithValidation.Core.Tests.Helpers;

namespace CopyWithValidation.Core.Tests;

public class FileCopierTests
{
    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_AnySize_DestinationMatchesSource(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        byte[] data = files.WriteSource(size);

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.Equal(data, File.ReadAllBytes(files.Destination));
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_MatchingCopy_ReturnsTrue(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        files.WriteSource(size);

        // Act
        bool result = await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_DestinationHashDiffers_ReturnsFalse(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new WrongHashFakeHasher());
        files.WriteSource(size);

        // Act
        bool result = await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_AnySize_LastProgressReportIs100(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        files.WriteSource(size);

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        // Assert
        Assert.Equal(100.0, progress.Values.Last());
    }

    [Fact]
    public async Task CopyAndVerifyAsync_MultiBufferFile_ReportsIncreasingProgress()
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        files.WriteSource(10 * 1024 * 1024);   // 10 MB

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        // Assert
        Assert.True(progress.Values.Count > 1);
        Assert.Equal(progress.Values.Order(), progress.Values);   // LINQ: sorted copy
    }

    [Fact]
    public async Task CopyAndVerifyAsync_DestinationExists_ThrowsIOException()
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        files.WriteSource(5);
        File.WriteAllText(files.Destination, "");   // destination already exists

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => copier.CopyAndVerifyAsync(files.Source, files.Destination));
    }

    [Fact]
    public async Task CopyAndVerifyAsync_SourceMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        using var files = new TempFiles();   // WriteSource not called, so no source file
        var copier = new FileCopier(new FileHasher());

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => copier.CopyAndVerifyAsync(files.Source, files.Destination));
        Assert.False(File.Exists(files.Destination));
    }
}