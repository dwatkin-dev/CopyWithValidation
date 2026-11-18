using CopyWithValidation.Core.Common;
using System.Security.Cryptography;

namespace CopyWithValidation.Core.Hashing;

internal static class StreamHasher
{
    private const int BufferSize = 1024 * 1024;

    private const int Alignment = 4096;
    public static async Task<string> HashAsync(Stream input, IProgress<double>? progress, Stream? copyTo = null, CancellationToken cancellationToken = default)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        long totalBytes = input.Length;

        cancellationToken.ThrowIfCancellationRequested();

        // Empty input: the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }

        long totalRead = 0;
        var buffer = AlignedBuffer.Allocate(BufferSize, Alignment);

        while (true)
        {
            int read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0) break;

            if (copyTo != null) await copyTo.WriteAsync(buffer[..read], cancellationToken).ConfigureAwait(false);
            hash.AppendData(buffer.Span[..read]);

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}