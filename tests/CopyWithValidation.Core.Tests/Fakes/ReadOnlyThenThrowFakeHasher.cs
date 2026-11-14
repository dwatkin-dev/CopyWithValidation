using CopyWithValidation.Core.Hashing;

public class ReadOnlyThenThrowFakeHasher : IFileHasher
{
    public Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        File.SetAttributes(path, FileAttributes.ReadOnly);
        return Task.FromException<string>(new IOException("Simulated read failure"));
    }
}