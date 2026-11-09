namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    private readonly IFileHasher _hasher = hasher;
    public bool CopyAndVerify(string source, string destination)
    {
        File.Copy(source, destination);

        return Verify(source, destination);
    }

    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<double>? progress = null)
    {
        await CopyWithProgressAsync(source, destination, progress);

        return Verify(source, destination);
    }

    private async Task CopyWithProgressAsync(string source, string destination, IProgress<double>? progress)
    {
        await using FileStream input = File.OpenRead(source);
        await using FileStream output = new(destination, FileMode.CreateNew);

        long totalBytes = input.Length;

        // Nothing to copy, but the caller still needs to see 100%
        if (totalBytes == 0)
        {
            progress?.Report(100.0);
            return;
        }

        long totalRead = 0;
        var buffer = new byte[1024 * 1024];

        while (true)
        {
            int read = await input.ReadAsync(buffer);
            if (read == 0) break;

            await output.WriteAsync(buffer.AsMemory(0, read));

            totalRead += read;
            progress?.Report(totalRead * 100.0 / totalBytes);
        }
    }

    private bool Verify(string source, string destination) => _hasher.ComputeHash(source) == _hasher.ComputeHash(destination);
}