namespace CopyWithValidation.Core.Hashing;

public class FileHasher(bool bypassCache = false) : IFileHasher
{
    private const FileOptions NoBuffering = (FileOptions)0x20000000;
    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0, bypassCache ? NoBuffering : FileOptions.None);

        return await StreamHasher.HashAsync(stream, progress, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
