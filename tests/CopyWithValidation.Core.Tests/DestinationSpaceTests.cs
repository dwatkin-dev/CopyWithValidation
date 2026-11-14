using CopyWithValidation.Core.Copying;
using CopyWithValidation.Core.Exceptions;

namespace CopyWithValidation.Core.Tests;

public class DestinationSpaceTests
{
    [Theory]
    [InlineData(0L, 0L)]
    [InlineData(100L, 100L)]   // exactly fills the drive
    [InlineData(100L, 200L)]
    public void EnsureFits_FileFitsFreeSpace_DoesNotThrow(long fileSize, long freeSpace)
    {
        DestinationSpace.EnsureFits(fileSize, freeSpace, "NTFS");
    }

    [Fact]
    public void EnsureFits_FileBiggerThanFreeSpace_ThrowsInsufficientSpace()
    {
        var ex = Assert.Throws<InsufficientSpaceException>(
            () => DestinationSpace.EnsureFits(101L, 100L, "NTFS"));

        Assert.Equal(101L, ex.Required);
        Assert.Equal(100L, ex.Available);
    }

    [Fact]
    public void EnsureFits_Fat32FileAtLimit_DoesNotThrow()
    {
        DestinationSpace.EnsureFits(4_294_967_295L, 10_000_000_000L, "FAT32");
    }

    [Fact]
    public void EnsureFits_Fat32FileOneByteOverLimit_ThrowsFileTooLarge()
    {
        var ex = Assert.Throws<FileTooLargeForFileSystemException>(
            () => DestinationSpace.EnsureFits(4_294_967_296L, 10_000_000_000L, "FAT32"));

        Assert.Equal(4_294_967_296L, ex.Required);
    }

    [Fact]
    public void EnsureFits_NtfsFileOver4GB_DoesNotThrow()
    {
        DestinationSpace.EnsureFits(5_000_000_000L, 10_000_000_000L, "NTFS");
    }

    [Fact]
    public void EnsureFits_Fat32FileTooLargeAndTooLittleSpace_ThrowsFileTooLarge()
    {
        Assert.Throws<FileTooLargeForFileSystemException>(
            () => DestinationSpace.EnsureFits(6_000_000_000L, 2_000_000_000L, "FAT32"));
    }
}