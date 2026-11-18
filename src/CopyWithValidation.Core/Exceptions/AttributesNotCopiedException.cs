namespace CopyWithValidation.Core.Exceptions;

public class AttributesNotCopiedException(string filePath, Exception innerException)
    : IOException($"Copy verified, but attributes could not be copied to {filePath}.", innerException)
{
    public string FilePath { get; } = filePath;
}
