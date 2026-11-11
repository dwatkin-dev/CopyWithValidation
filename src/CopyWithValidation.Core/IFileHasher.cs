namespace CopyWithValidation.Core;

/// <summary>
/// Anything that can hash a file. An interface is a promise of members with no code;
/// FileCopier depends on this, not on FileHasher, so tests can pass in a fake.
/// </summary>
public interface IFileHasher
{
    /// <summary>Returns the file's SHA-256 hash as lower-case hex, reporting 0–100 progress.</summary>
    // Interface members are public by default.
    // Task<string> = a string that will arrive later; the caller awaits it.
    // IProgress<double>? = optional listener ('?' allows null; '= null' lets callers leave it out).
    Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null);
}
