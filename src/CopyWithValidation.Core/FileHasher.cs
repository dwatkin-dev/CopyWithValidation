using System.Security.Cryptography;

namespace CopyWithValidation.Core;

public class FileHasher : IFileHasher
{
    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null)
    {
        await using FileStream stream = File.OpenRead(path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        
        long totalBytes = stream.Length;

        // Nothing to hash, but the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }

        var buffer = new byte[1024 * 1024];
        long totalRead = 0;

        while (true)
        {
            int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
            if (read == 0) break;

            hash.AppendData(buffer, 0, read);

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}
