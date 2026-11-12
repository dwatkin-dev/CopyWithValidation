namespace CopyWithValidation.Core.Tests.Fakes;

public class WrongHashFakeHasher : IFileHasher
{
    public Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null) => Task.FromResult("not-a-real-hash");
}
