using CopyWithValidation.Core.Common;
using CopyWithValidation.Core.Copying;
using CopyWithValidation.Core.Exceptions;
using CopyWithValidation.Core.Hashing;
using CopyWithValidation.Core.Tests.Fakes;
using CopyWithValidation.Core.Tests.Helpers;
using System.Security.Cryptography;

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
    public async Task CopyAndVerifyAsync_MatchingCopy_LeavesNoCorruptFile(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new FileHasher());
        files.WriteSource(size);

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.False(File.Exists(files.Destination + ".corrupt"));
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_DestinationHashDiffers_RenamesToCorrupt(int size)
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new WrongHashFakeHasher());
        files.WriteSource(size);
        string corruptPath = files.Destination + ".corrupt";

        // Act
        var ex = await Assert.ThrowsAsync<VerificationFailedException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

        // Assert
        Assert.Equal(corruptPath, ex.CorruptFilePath);
        Assert.True(File.Exists(corruptPath));
        Assert.False(File.Exists(files.Destination));
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

        // Assert
        var sorted = progress.Values
            .OrderBy(r => r.Phase)     // Copying before Verifying
            .ThenBy(r => r.Percent);   // then low to high within each phase
        Assert.True(progress.Values.Count > 1);
        Assert.Equal(sorted, progress.Values);   // LINQ: sorted copy
    }

    [Fact]
    public async Task CopyAndVerifyAsync_SourceMissing_ThrowsAndLeavesNoDestination()
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

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    public async Task CopyAndVerifyAsync_AlreadyCancelled_ThrowsAndLeavesNoDestination(int size)
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(size);
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

    [Fact]
    public async Task AnyFile_DestinationKeepsSourceLastWriteTime()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        var expected = new DateTime(2020, 1, 2, 3, 4, 6, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(files.Source, expected);
        var copier = new FileCopier(new FileHasher());

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.Equal(expected, File.GetLastWriteTimeUtc(files.Destination));
    }

    [Fact]
    public async Task AnyFile_DestinationKeepsSourceCreationTime()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        var expected = new DateTime(2020, 1, 2, 3, 4, 6, DateTimeKind.Utc);
        File.SetCreationTimeUtc(files.Source, expected);
        var copier = new FileCopier(new FileHasher());

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.Equal(expected, File.GetCreationTimeUtc(files.Destination));
    }

    [Fact]
    public async Task VerifyFails_ThrowsAndLeavesNoDestination()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        var copier = new FileCopier(new ThrowingFakeHasher());

        // Act + Assert
        await Assert.ThrowsAsync<IOException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));
        Assert.False(File.Exists(files.Destination));
    }

    [Fact]
    public async Task CleanupFails_ThrowsOriginalException()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        var copier = new FileCopier(new ReadOnlyThenThrowFakeHasher());

        // Act + Assert
        await Assert.ThrowsAsync<IOException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

        // Clear read-only so TempFiles.Dispose can delete it
        File.SetAttributes(files.Destination, FileAttributes.Normal);
    }

    [Fact]
    public async Task CopyAndVerifyAsync_DestinationHashDiffers_ExceptionHasBothHashes()
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new WrongHashFakeHasher());
        byte[] data = files.WriteSource(5);
        string expectedSourceHash = Convert.ToHexStringLower(SHA256.HashData(data));   // oracle

        // Act
        var ex = await Assert.ThrowsAsync<VerificationFailedException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

        // Assert
        Assert.Equal(expectedSourceHash, ex.SourceHash);
        Assert.Equal("not-a-real-hash", ex.DestinationHash);
    }

    [Fact]
    public async Task CopyAndVerifyAsync_CorruptFileAlreadyExists_ReplacesIt()
    {
        // Arrange
        using var files = new TempFiles();
        var copier = new FileCopier(new WrongHashFakeHasher());
        byte[] data = files.WriteSource(5);
        string corruptPath = files.Destination + ".corrupt";
        File.WriteAllText(corruptPath, "old");   // left over from an earlier failed run

        // Act
        await Assert.ThrowsAsync<VerificationFailedException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

        // Assert
        Assert.Equal(data, File.ReadAllBytes(corruptPath));
    }

    [Fact]
    public async Task CopyAndVerifyAsync_DestinationExists_ThrowsAndLeavesFileUntouched()
    {
        // Arrange
        using var files = new TempFiles();
        File.WriteAllText(files.Source, "new content");
        File.WriteAllText(files.Destination, "user's original file");
        var copier = new FileCopier(new FileHasher());

        // Act
        var ex = await Assert.ThrowsAsync<DestinationExistsException>(
            () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

        // Assert
        Assert.Equal(files.Destination, ex.FilePath);
        Assert.Equal("user's original file", File.ReadAllText(files.Destination));
    }

    [Fact]
    public async Task AnyFile_DestinationKeepsSourceAttributes()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        File.SetAttributes(files.Source, FileAttributes.Hidden);
        var copier = new FileCopier(new FileHasher());

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination);

        // Assert
        Assert.True(File.GetAttributes(files.Destination).HasFlag(FileAttributes.Hidden));
    }

    [Fact]
    public async Task VerifyFails_CorruptFileIsNotReadOnly()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(5);
        File.SetAttributes(files.Source, FileAttributes.ReadOnly);
        var copier = new FileCopier(new WrongHashFakeHasher());
        string corruptPath = files.Destination + ".corrupt";

        try
        {
            // Act
            await Assert.ThrowsAsync<VerificationFailedException>(
                () => copier.CopyAndVerifyAsync(files.Source, files.Destination));

            // Assert
            Assert.False(File.GetAttributes(corruptPath).HasFlag(FileAttributes.ReadOnly));
        }
        finally
        {
            // Clean up
            File.SetAttributes(files.Source, FileAttributes.Normal);
        }
    }
}