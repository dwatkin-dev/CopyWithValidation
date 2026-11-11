namespace CopyWithValidation.Core;

/// <summary>The real hasher: reads a file from disk and hashes it with SHA-256.</summary>
public class FileHasher : IFileHasher
{
    /// <inheritdoc />
    public async Task<string> ComputeHashAsync(string path, IProgress<double>? progress = null)
    {
        // 'await using' closes the file when the method ends, using the stream's async
        // DisposeAsync. Throws FileNotFoundException if the path doesn't exist.
        await using FileStream stream = File.OpenRead(path);

        // 'return await' (not just 'return') keeps us inside this method – and so keeps the
        // stream open – until hashing has finished. Returning the bare Task would close it early.
        // ConfigureAwait(false): Core never touches the UI, so it needn't resume on the UI thread.
        return await StreamHasher.HashAsync(stream, progress).ConfigureAwait(false);
    }
}
