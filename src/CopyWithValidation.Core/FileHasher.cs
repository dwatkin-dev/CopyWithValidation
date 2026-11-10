using System.Security.Cryptography;

namespace CopyWithValidation.Core;

public class FileHasher : IFileHasher
{
    public string ComputeHash(Stream stream) => Convert.ToHexStringLower(SHA256.HashData(stream));

    public string ComputeHash(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return ComputeHash(stream);
    }

    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null)
    {
        await using FileStream stream = File.OpenRead(path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        
        long totalBytes = stream.Length;

        // Nothing to copy, but the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }

        var buffer = new byte[1024 * 1024];
        long totalRead = 0;

        while (true)
        {
            int read = await stream.ReadAsync(buffer);
            if (read == 0) break;

            hash.AppendData(buffer, 0, read);

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}
    //private async Task<string> CopyWithProgressAsync(string source, string destination, IProgress<double>? progress)
    //{
    //    await using FileStream input = File.OpenRead(source);
    //    await using FileStream output = new(destination, FileMode.CreateNew);
    //    using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    //    long totalBytes = input.Length;

    //    // Nothing to copy, but the caller still needs to see 100%
    //    if (totalBytes == 0)
    //    {
    //        progress?.Report(100.0);
    //        return Convert.ToHexStringLower(hash.GetHashAndReset());
    //    }

    //    long totalRead = 0;
    //    var buffer = new byte[1024 * 1024];

    //    while (true)
    //    {
    //        int read = await input.ReadAsync(buffer);
    //        if (read == 0) break;

    //        await output.WriteAsync(buffer.AsMemory(0, read));
    //        hash.AppendData(buffer, 0, read);

    //        totalRead += read;
    //        progress?.Report(totalRead * 100.0 / totalBytes);
    //    }

    //    return Convert.ToHexStringLower(hash.GetHashAndReset());
    //}