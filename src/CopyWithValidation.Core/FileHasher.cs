using System.Security.Cryptography;

namespace CopyWithValidation.Core;
public class FileHasher
{
    public string ComputeHash(Stream stream) => Convert.ToHexStringLower(SHA256.HashData(stream));

    public string ComputeHash(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return ComputeHash(stream);
    }
}