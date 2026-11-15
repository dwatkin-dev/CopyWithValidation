namespace CopyWithValidation.Core.Copying;

public static class DestinationPath
{
    public static string Build(string sourceFile, string destinationFolder)
    {
        string fileName = Path.GetFileName(sourceFile);

        if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("Invalid source file", nameof(sourceFile));
        
        return Path.Combine(destinationFolder, fileName);
    }
}
