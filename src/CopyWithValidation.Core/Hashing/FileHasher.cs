namespace CopyWithValidation.Core.Hashing;

public class FileHasher : IFileHasher
{
    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(path);

        return await StreamHasher.HashAsync(stream, progress, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
