namespace CopyWithValidation.Core;

/// <summary>Copies a file, then checks the copy by comparing SHA-256 hashes.</summary>
// Primary constructor + constructor injection: whoever creates a FileCopier chooses the
// hasher (FileHasher in the app, a fake in tests). 'hasher' is usable in every method.
public class FileCopier(IFileHasher hasher)
{
    /// <summary>
    /// Copies <paramref name="source"/> to <paramref name="destination"/> and verifies it.
    /// Reports progress as Copying 0–100, then Verifying 0–100.
    /// </summary>
    /// <returns>True if the hashes match. Copy problems (missing source, destination exists) throw.</returns>
    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<CopyProgress>? progress = null)
    {
        // Two adapters. The copy loop and the hasher only report plain numbers (double).
        // Each adapter is a lambda that wraps the number in a CopyProgress "envelope" with its
        // phase, then passes it on to the caller's listener. Building them doesn't run them –
        // they run later, each time the loop calls Report. They 'capture' the progress variable.
        var copyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Copying, percent)));
        var verifyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Verifying, percent)));

        // The source hash is worked out WHILE copying, so the source is only read once.
        // await on a Task<string> gives the string.
        string sourceHash = await CopyAsync(source, destination, copyProgress).ConfigureAwait(false);

        return await VerifyAsync(sourceHash, destination, verifyProgress).ConfigureAwait(false);
    }

    /// <summary>Copies the file and returns the SOURCE file's hash.</summary>
    private async Task<string> CopyAsync(string source, string destination, IProgress<double>? progress)
    {
        await using FileStream input = File.OpenRead(source);
        // FileMode.CreateNew throws IOException if the destination already exists (no overwrite).
        // 'new(...)' is target-typed new: the type comes from the left-hand side.
        await using FileStream output = new(destination, FileMode.CreateNew);

        // 'return await' keeps both streams open until the copy has finished.
        // copyTo: output is a named argument – says what this argument is for.
        return await StreamHasher.HashAsync(input, progress, copyTo: output).ConfigureAwait(false);
    }

    /// <summary>Re-reads the destination from disk and checks its hash matches the source's.</summary>
    // Expression-bodied member (=>): a one-expression method body, no braces or 'return'.
    private async Task<bool> VerifyAsync(string sourceHash, string destination, IProgress<double>? progress)
        => sourceHash == await hasher.ComputeHashAsync(destination, progress).ConfigureAwait(false);
}