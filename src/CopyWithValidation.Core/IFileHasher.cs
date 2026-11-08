namespace CopyWithValidation.Core;

public interface IFileHasher
{
    public string ComputeHash(string path);
}
