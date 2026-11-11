

namespace CopyWithValidation.Core;

public class FileCopier(IFileHasher hasher)
{
    public async Task<bool> CopyAndVerifyAsync(string source, string destination, IProgress<CopyProgress>? progress = null)
    {
        var copyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Copying, percent)));
        var verifyProgress = new ActionProgress<double>(percent => progress?.Report(new CopyProgress(CopyPhase.Verifying, percent)));

        string sourceHash = await CopyAsync(source, destination, copyProgress).ConfigureAwait(false);

        return await VerifyAsync(sourceHash, destination, verifyProgress).ConfigureAwait(false);
    }

    private async Task<string> CopyAsync(string source, string destination, IProgress<double>? progress)
    {
        await using FileStream input = File.OpenRead(source);
        await using FileStream output = new(destination, FileMode.CreateNew);

        return await StreamHasher.HashAsync(input, progress, copyTo: output).ConfigureAwait(false);
    }

    private async Task<bool> VerifyAsync(string sourceHash, string destination, IProgress<double>? progress) 
        => sourceHash == await hasher.ComputeHashAsync(destination, progress).ConfigureAwait(false);
}