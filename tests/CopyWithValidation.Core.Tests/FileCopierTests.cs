using CopyWithValidation.Core.Tests.Fakes;
using CopyWithValidation.Core.Tests.Helpers;

namespace CopyWithValidation.Core.Tests;

/// <summary>Tests for FileCopier: the copy itself, the verify result, progress, and errors.</summary>
// Test basics (xUnit):
// - [Fact] = one test. [Theory] + [InlineData(x)] = the same test run once per value of x.
// - Names: Method_Scenario_ExpectedResult. Body: Arrange / Act / Assert (AAA).
// - Tests of async code are 'async Task' and await the call, so failures are seen.
// - Each test checks ONE behaviour; sizes cover the boundaries (empty, < 1 buffer, > 1 buffer).
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
        Assert.Equal(data, File.ReadAllBytes(files.Destination));   // compares byte by byte
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
        // The fake always returns a wrong destination hash, so verification must fail.
        // Possible only because FileCopier takes an IFileHasher (constructor injection).
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
        // Select (LINQ) turns each CopyProgress into just its Percent
        var percents = progress.Values.Select(r => r.Percent).ToList();

        // Assert
        // "In order" = phases never go back AND percent only rises within a phase.
        // Sorting by both and checking nothing moved tests both at once.
        var sorted = progress.Values
            .OrderBy(r => r.Phase)     // Copying before Verifying
            .ThenBy(r => r.Percent);   // then low to high within each phase
        Assert.True(percents.Count > 1);
        // Lists compare item by item; each item uses the record's value equality
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
        // ThrowsAsync takes a lambda: () => ... is a method with no parameters that xUnit
        // calls itself, so it can catch the exception. It's awaited because the call is async.
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
        // The source is opened first, so no empty destination file should be left behind
        Assert.False(File.Exists(files.Destination));
    }

    [Fact]
    public async Task CopyAndVerifyAsync_AnyFile_FirstReportIsCopying()
    {
        // Arrange
        using var files = new TempFiles();
        files.WriteSource(1024);
        var progress = new RecordingProgress<CopyProgress>();
        var copier = new FileCopier(new FileHasher());

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        // Assert
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
        var copier = new FileCopier(new FileHasher());   // real hasher – the fake reports no progress

        // Act
        await copier.CopyAndVerifyAsync(files.Source, files.Destination, progress);

        // Assert
        // Works because records compare by value: a NEW CopyProgress equals the recorded one
        Assert.Equal(new CopyProgress(CopyPhase.Verifying, 100), progress.Values.Last());
    }
}