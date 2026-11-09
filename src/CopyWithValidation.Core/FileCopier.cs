namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    private readonly IFileHasher _hasher = hasher;
    public bool CopyAndVerify(string source, string destination)
    {
        File.Copy(source, destination);

        return _hasher.ComputeHash(source) == _hasher.ComputeHash(destination);
    }

    public async Task<bool> CopyAndVerifyAsync(string source, string destination)
    {
        bool verified = await Task.Run(() => CopyAndVerify(source, destination));
        return verified;
    }
}