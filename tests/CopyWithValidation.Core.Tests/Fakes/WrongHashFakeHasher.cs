using CopyWithValidation.Core.Hashing;

namespace CopyWithValidation.Core.Tests.Fakes;

public class WrongHashFakeHasher : IFileHasher
{
    public Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult("not-a-real-hash");
}
