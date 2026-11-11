namespace CopyWithValidation.Core.Tests.Fakes;

/// <summary>
/// Fake hasher that always returns a wrong hash, to test the "verification failed" path
/// without having to corrupt a real file. Reports no progress.
/// </summary>
public class WrongHashFakeHasher : IFileHasher
{
    // Not async: there's nothing to wait for. Task.FromResult wraps a value in an
    // already-finished Task, so it still matches the interface's Task<string>.
    public Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null) => Task.FromResult("not-a-real-hash");
}
