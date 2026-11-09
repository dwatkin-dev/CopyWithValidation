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
        await using FileStream input = File.OpenRead(source);
        await using FileStream output = new FileStream(destination, FileMode.CreateNew);

        var buffer = new byte[1024 * 1024];
        int read = 0;
        long totalRead = 0;

        do
        {
            read = await input.ReadAsync(buffer);
            await output.WriteAsync(buffer.AsMemory(0, read));
            totalRead += read;
            progress?.Report(totalRead * 100.0 / input.Length);
        } while (read > 0);

        input.Close();
        output.Close();

        return _hasher.ComputeHash(source) == _hasher.ComputeHash(destination);
    }
}