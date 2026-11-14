using CopyWithValidation.Core.Exceptions;

namespace CopyWithValidation.Core.Copying;

public static class DestinationSpace
{
    private const long Fat32MaxFileSize = 4_294_967_295;
    public static void EnsureFits(long fileSize, long freeSpace, string driveFormat)
    {
        if (driveFormat == "FAT32" && fileSize > Fat32MaxFileSize) throw new FileTooLargeForFileSystemException(fileSize, Fat32MaxFileSize, driveFormat);

        if (fileSize > freeSpace) throw new InsufficientSpaceException(fileSize, freeSpace);
    }
}
