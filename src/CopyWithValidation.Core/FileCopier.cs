namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    private readonly IFileHasher _hasher = hasher;
    public bool CopyAndVerify(string source, string destination)
    {
        File.Copy(source, destination);

        return _hasher.ComputeHash(source) == _hasher.ComputeHash(destination);
    }

    public Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<double>? progress = null)
    {
        progress?.Report(100.0); // ? is null check
        return Task.Run(() => CopyAndVerify(source, destination));
    }
}