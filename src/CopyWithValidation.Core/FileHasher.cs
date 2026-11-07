using System.Security.Cryptography;

namespace CopyWithValidation.Core;
public class FileHasher
{
    public string ComputeHash(Stream stream) => Convert.ToHexStringLower(SHA256.HashData(stream));
}