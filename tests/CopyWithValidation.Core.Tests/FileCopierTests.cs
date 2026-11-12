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

    [Fact]
    public async Task CopyAndVerifyAsync_MultiBufferFile_ReportsProgressInOrder()
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<CopyProgress>();
        files.WriteSource(10 * 1024 * 1024);   // 10 MB

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);
        var percents = progress.Values.Select(r => r.Percent).ToList();

        // Assert
        var sorted = progress.Values
            .OrderBy(r => r.Phase)     // Copying before Verifying
            .ThenBy(r => r.Percent);   // then low to high within each phase
        Assert.True(percents.Count > 1);
        Assert.Equal(sorted, progress.Values);   // LINQ: sorted copy
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

    [Fact]
    public async Task CopyAndVerifyAsync_AnyFile_FirstReportIsCopying()
    {
        using var files = new TempFiles();
        files.WriteSource(1024);
        var progress = new RecordingProgress<CopyProgress>();
        var copier = new FileCopier(new FileHasher());

        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        Assert.Equal(CopyPhase.Copying, progress.Values[0].Phase);
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_AnyFile_LastReportIsVerifying100(int size)
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(size);
        var progress = new RecordingProgress<CopyProgress>();
        var copier = new FileCopier(new FileHasher());

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        // Assert
        Assert.Equal(new CopyProgress(CopyPhase.Verifying, 100), progress.Values.Last());
    }

    [Fact]
    public async Task CopyAndVerifyAsync_AlreadyCancelled_ThrowsAndLeavesNoDestination()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        var copier = new FileCopier(new FileHasher());
        var token = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination, cancellationToken: token));
        Assert.False(File.Exists(files.Destination));
    }

    [Theory]
    [InlineData(CopyPhase.Copying)]
    [InlineData(CopyPhase.Verifying)]
    public async Task CopyAndVerifyAsync_CancelledDuringPhase_ThrowsAndLeavesNoDestination(CopyPhase phase)
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(10 * 1024 * 1024);   // 10 MB, several buffers
        var copier = new FileCopier(new FileHasher());
        using var cts = new CancellationTokenSource();
        var progress = new ActionProgress<CopyProgress>(p =>
        {
            if (p.Phase == phase) cts.Cancel();
        });

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination, progress, cts.Token));
        Assert.False(File.Exists(files.Destination));
    }
}