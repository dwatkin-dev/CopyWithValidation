namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<CopyProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var copyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Copying, percent)));
        var verifyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Verifying, percent)));
        try
        {
            string sourceHash = await CopyAsync(source, destination, copyProgress, cancellationToken).ConfigureAwait(false);

            CopyTimestamps(source, destination);

            return await VerifyAsync(sourceHash, destination, verifyProgress, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            File.Delete(destination);
            throw;
        }
    }

    private async Task<string> CopyAsync(string source, string destination, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        await using FileStream input = File.OpenRead(source);
        await using FileStream output = new(destination, FileMode.CreateNew);

        return await StreamHasher.HashAsync(input, progress, copyTo: output, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> VerifyAsync(string sourceHash, string destination, IProgress<double>? progress, CancellationToken cancellationToken) 
        => sourceHash == await hasher.ComputeHashAsync(destination, progress, cancellationToken: cancellationToken).ConfigureAwait(false);

    private static void CopyTimestamps(string source, string destination)
    {
        File.SetCreationTimeUtc(destination, File.GetCreationTimeUtc(source));
        File.SetLastWriteTimeUtc(destination, File.GetLastWriteTimeUtc(source));
    }
}