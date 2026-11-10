using CopyWithValidation.Core.Tests.Fakes;

namespace CopyWithValidation.Core.Tests;

public class FileCopierTests
{
    [Fact]
    public async Task CopyAndVerifyAsync_LargeFile_ReportsIncreasingProgress()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        File.WriteAllBytes(source, new byte[10 * 1024 * 1024]);   // 10 MB of zeros

        try
        {
            // Act
            await copier.CopyAndVerifyAsync(source, destination, progress);

            // Assert
            Assert.True(progress.Values.Count > 1);
            Assert.Equal(progress.Values.Order(), progress.Values);   // LINQ: sorted copy
            Assert.Equal(100.0, progress.Values.Last());
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_AnySize_DestinationMatchesSource(int size)
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var data = new byte[size];
            new Random(42).NextBytes(data);
            File.WriteAllBytes(source, data);

            // Act
            await copier.CopyAndVerifyAsync(source, destination);

            // Assert
            Assert.Equal(data, File.ReadAllBytes(destination));
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_MatchingCopy_ReturnsTrue(int size)
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var data = new byte[size];
            new Random(42).NextBytes(data);
            File.WriteAllBytes(source, data);

            // Act
            bool result = await copier.CopyAndVerifyAsync(source, destination);

            // Assert
            Assert.True(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_DestinationHashDiffers_ReturnsFalse(int size)
    {
        // Arrange
        var copier = new FileCopier(new WrongHashFakeHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var data = new byte[size];
            new Random(42).NextBytes(data);
            File.WriteAllBytes(source, data);

            // Act
            bool result = await copier.CopyAndVerifyAsync(source, destination);

            // Assert
            Assert.False(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Theory]
    [InlineData(0)]                        // empty
    [InlineData(5)]                        // one partial buffer
    [InlineData(1024 * 1024 + 512 * 1024)] // several buffers, partial last
    public async Task CopyAndVerifyAsync_AnySize_LastProgressReportIs100(int size)
    {
        // Arrange
        var copier = new FileCopier(new WrongHashFakeHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var data = new byte[size];
            new Random(42).NextBytes(data);
            File.WriteAllBytes(source, data);

            // Act
            await copier.CopyAndVerifyAsync(source, destination, progress);

            // Assert
            Assert.Equal(100.0, progress.Values.Last());
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }
}