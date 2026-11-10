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

    public async Task<string> ComputeHashAsync(string source)
    {
        await using FileStream stream = File.OpenRead(source);
        byte[] data = await SHA256.HashDataAsync(stream);
        return Convert.ToHexStringLower(data);
    }
}