namespace CopyWithValidation.Core;

public class FileTooLargeForFileSystemException(long required, long limit, string fileSystem)
    : IOException($"File too large for {fileSystem} file system: needs {required:N0} bytes, {fileSystem} limit is {limit:N0}.")
{
    public long Required { get; } = required;
    public string FileSystem { get; } = fileSystem;
    public long Limit { get; } = limit;
}