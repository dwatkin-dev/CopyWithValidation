using System.Security.Cryptography;

namespace CopyWithValidation.Core;

public class FileHasher : IFileHasher
{
    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null)
    {
        await using FileStream stream = File.OpenRead(path);

        return await StreamHasher.HashAsync(stream, progress).ConfigureAwait(false);
    }
}
