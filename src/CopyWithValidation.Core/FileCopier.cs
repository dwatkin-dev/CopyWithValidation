using System.Security.Cryptography;

namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<CopyProgress>? progress = null)
    {
        var copyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Copying, percent)));
        var verifyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Verifying, percent)));

        string sourceHash = await CopyWithProgressAsync(source, destination, copyProgress);

        return await VerifyAsync(sourceHash, destination, verifyProgress);
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
            return Convert.ToHexStringLower(hash.GetHashAndReset());
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

    private async Task<bool> VerifyAsync(string sourceHash, string destination, IProgress<double>? progress) 
        => sourceHash == await hasher.ComputeHashAsync(destination, progress);
}