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
        progress?.Report(100.0);
        await using FileStream stream = File.OpenRead(path);
        byte[] data = await SHA256.HashDataAsync(stream);
        return Convert.ToHexStringLower(data);
    }
}