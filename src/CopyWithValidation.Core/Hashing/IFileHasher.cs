namespace CopyWithValidation.Core.Hashing;

public interface IFileHasher
{
    Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
