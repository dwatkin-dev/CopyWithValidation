using CopyWithValidation.Core.Tests.Fakes;

namespace CopyWithValidation.Core.Tests;

public class FileCopierTests
{
    [Fact]
    public void CopyAndVerify_ValidSource_CreatesIdenticalDestination()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            File.WriteAllText(source, "hello");

            // Act
            copier.CopyAndVerify(source, destination);

            // Assert
            Assert.Equal("hello", File.ReadAllText(destination));
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Fact]
    public void CopyAndVerify_ValidSource_ReturnsTrue()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            File.WriteAllText(source, "hello");

            // Act
            bool result = copier.CopyAndVerify(source, destination);

            // Assert
            Assert.True(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Fact]
    public void CopyAndVerify_HashesDiffer_ReturnsFalse()
    {
        // Arrange
        var copier = new FileCopier(new MismatchingFakeHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            File.WriteAllText(source, "hello");

            // Act
            bool result = copier.CopyAndVerify(source, destination);

            // Assert
            Assert.False(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Fact]
    public async Task CopyAndVerifyAsync_ValidFile_ReturnsTrue()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        File.WriteAllText(source, "hello");

        try
        {
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

    [Fact]
    public async Task CopyAndVerifyAsync_WithProgress_LastReportIs100()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        File.WriteAllText(source, "hello");

        try
        {
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

    [Fact]
    public async Task CopyAndVerifyAsync_EmptyFile_LastReport_Is100()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName(); // GetTempFileName creates an empty file
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
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

    [Fact]
    public async Task CopyAndVerifyAsync_DestinationHashDiffers_ReturnsFalse()
    {
        // Arrange
        var copier = new FileCopier(new WrongHashFakeHasher());
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        File.WriteAllText(source, "abc");

        try
        {
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

    [Fact]
    public async Task CopyAndVerifyAsync_EmptyFile_ReturnsTrue()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName(); // GetTempFileName creates an empty file
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            bool result = await copier.CopyAndVerifyAsync(source, destination, progress);

            // Assert
            Assert.True(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }

    [Fact]
    public async Task CopyAndVerifyAsync_MultiBufferFile_ReturnsTrue()
    {
        // Arrange
        var copier = new FileCopier(new FileHasher());
        var progress = new RecordingProgress<double>();
        string source = Path.GetTempFileName();
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var data = new byte[1024 * 1024 + 512 * 1024];   // 1.5 MB
        new Random(42).NextBytes(data);                  // same "random" bytes every run
        File.WriteAllBytes(source, data);

        try
        {
            // Act
            bool result = await copier.CopyAndVerifyAsync(source, destination, progress);

            // Assert
            Assert.True(result);
        }
        finally
        {
            File.Delete(source);
            File.Delete(destination);
        }
    }
}
