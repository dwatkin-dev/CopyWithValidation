using System.Security.Cryptography;

namespace CopyWithValidation.Core;

/// <summary>
/// The shared read loop used by both FileHasher and FileCopier: reads a stream in
/// 1 MB chunks, hashes each chunk, optionally copies it, and reports progress.
/// </summary>
// internal = visible only inside Core (not the App or tests); it's tested through
// FileHasher and FileCopier. static = no objects are made; call StreamHasher.HashAsync(...).
internal static class StreamHasher
{
    /// <summary>
    /// Hashes the whole of <paramref name="input"/> with SHA-256 and returns lower-case hex.
    /// If <paramref name="copyTo"/> is given, each chunk is also written to it.
    /// </summary>
    // 'async' lets the method use 'await'. It does NOT start a new thread – it runs on the
    // caller's thread until the first unfinished await, then gives the thread back.
    // 'Stream? copyTo = null' is an optional parameter (must come last). Callers can name it:
    // HashAsync(input, progress, copyTo: output).
    public static async Task<string> HashAsync(Stream input, IProgress<double>? progress, Stream? copyTo = null)
    {
        // IncrementalHash builds the hash a chunk at a time, so the whole file is never
        // in memory at once. 'using var' disposes it when the method ends.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        long totalBytes = input.Length;   // long, not int – files can be over 2 GB

        // Empty input: the caller still needs to see 100%
        // (and it avoids 0 / 0 below, which for doubles gives NaN, not an exception)
        if (totalBytes == 0)
        {
            progress?.Report(100.0);   // ?. = only call Report if progress isn't null
            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }

        long totalRead = 0;
        var buffer = new byte[1024 * 1024];   // 1 MB, reused for every chunk

        while (true)
        {
            // Reads UP TO buffer.Length bytes; 'read' is how many we actually got.
            // At 'await' the thread is handed back while the disk works; the method resumes after.
            int read = await input.ReadAsync(buffer).ConfigureAwait(false);
            if (read == 0) break;   // 0 bytes = end of the stream

            // AsMemory(0, read) / (buffer, 0, read): only use the bytes just read –
            // the last chunk is usually smaller than the buffer, and the rest is old data.
            if (copyTo != null) await copyTo.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
            hash.AppendData(buffer, 0, read);

            totalRead += read;
            // * 100.0 makes this double maths (integer division would round down to 0)
            progress?.Report(totalRead * 100.0 / totalBytes);
        }

        // Finish the hash and turn its bytes into a hex string like "ba7816bf..."
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}