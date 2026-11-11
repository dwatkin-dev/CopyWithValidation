using System.Security.Cryptography;

namespace CopyWithValidation.Core;

internal static class StreamHasher
{
    public static async Task<string> HashAsync(Stream input, IProgress<double>? progress, Stream? copyTo = null)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        long totalBytes = input.Length;

        // Nothing to copy, but the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }

        long totalRead = 0;
        var buffer = new byte[1024 * 1024];

        while (true)
        {
            int read = await input.ReadAsync(buffer).ConfigureAwait(false);
            if (read == 0) break;

            if (copyTo != null) await copyTo.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
            hash.AppendData(buffer, 0, read);

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}