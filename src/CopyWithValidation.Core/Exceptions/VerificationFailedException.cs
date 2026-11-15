namespace CopyWithValidation.Core.Exceptions;

public class VerificationFailedException(string sourceHash, string destinationHash, string corruptFilePath) 
    : IOException($"Copy failed verification. Kept as {corruptFilePath} for inspection.")
{
    public string SourceHash { get; } = sourceHash;
    public string DestinationHash { get; } = destinationHash;
    public string CorruptFilePath { get; } = corruptFilePath;
}
