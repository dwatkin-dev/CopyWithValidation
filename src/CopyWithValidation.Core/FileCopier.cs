using System.Security.Cryptography;

namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    private readonly IFileHasher _hasher = hasher;
    public bool CopyAndVerify(string source, string destination)
    {
        File.Copy(source, destination);

        return _hasher.ComputeHash(source) == _hasher.ComputeHash(destination);
    }

    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<double>? progress = null)
    {
        string sourceHash = await CopyWithProgressAsync(source, destination, progress);

        return Verify(sourceHash, destination);
    }

    private async Task<string> CopyWithProgressAsync(string source, string destination, IProgress<double>? progress)
    {
        await using FileStream input = File.OpenRead(source);
        await using FileStream output = new(destination, FileMode.CreateNew);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        long totalBytes = input.Length;

        // Nothing to copy, but the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return "";
        }

        long totalRead = 0;
        var buffer = new byte[1024 * 1024];

        while (true)
        {
            int read = await input.ReadAsync(buffer);
            if (read == 0) break;

            await output.WriteAsync(buffer.AsMemory(0, read));
            hash.AppendData(buffer, 0, read);

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private bool Verify(string sourceHash, string destination) => sourceHash == _hasher.ComputeHash(destination);
}