namespace CopyWithValidation.Core;

public class FileCopier
{
    public bool CopyAndVerify(string source, string destination)
    {
        var hasher = new FileHasher();
        File.Copy(source, destination);

        return hasher.ComputeHash(source) == hasher.ComputeHash(destination);
    }
}