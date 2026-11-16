namespace CopyWithValidation.Core.Exceptions;

public class DestinationExistsException(string filePath)
    : IOException($"Destination already exists: {filePath}")
{
    public string FilePath { get; } = filePath;
}
