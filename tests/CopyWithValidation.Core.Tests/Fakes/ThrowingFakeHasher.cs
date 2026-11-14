using CopyWithValidation.Core.Hashing;

namespace CopyWithValidation.Core.Tests.Fakes;

public class ThrowingFakeHasher : IFileHasher
{
    public Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
        => Task.FromException<string>(new IOException("Simulated read failure"));
}