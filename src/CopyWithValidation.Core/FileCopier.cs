namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<CopyProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var copyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Copying, percent)));
        var verifyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Verifying, percent)));

        EnsureDestinationHasRoom(source, destination);

        FileStream output = new(destination, FileMode.CreateNew);

        try
        {
            string sourceHash;

            await using (output)
            {
                sourceHash = await CopyAsync(source, output, copyProgress, cancellationToken).ConfigureAwait(false);
            }

            CopyTimestamps(source, destination);

            return await VerifyAsync(sourceHash, destination, verifyProgress, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            TryDeleteDestination(destination);
            throw;
        }
    }

    private async Task<string> CopyAsync(string source, Stream output, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        await using FileStream input = File.OpenRead(source);

        return await StreamHasher.HashAsync(input, progress, copyTo: output, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> VerifyAsync(string sourceHash, string destination, IProgress<double>? progress, CancellationToken cancellationToken) 
        => sourceHash == await hasher.ComputeHashAsync(destination, progress, cancellationToken: cancellationToken).ConfigureAwait(false);

    private static void CopyTimestamps(string source, string destination)
    {
        File.SetCreationTimeUtc(destination, File.GetCreationTimeUtc(source));
        File.SetLastWriteTimeUtc(destination, File.GetLastWriteTimeUtc(source));
    }

    private static void TryDeleteDestination(string path)
    {
        try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    private static void EnsureDestinationHasRoom(string source, string destination)
    {
        long fileSize = new FileInfo(source).Length;
        string? root = Path.GetPathRoot(Path.GetFullPath(destination)) ?? throw new ArgumentException("Destination has no root.", nameof(destination));
        DriveInfo drive = new DriveInfo(root);

        DestinationSpace.EnsureFits(fileSize, drive.AvailableFreeSpace, drive.DriveFormat);
    }
}