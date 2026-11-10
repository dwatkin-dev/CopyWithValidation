namespace CopyWithValidation.Core;

public interface IFileHasher
{
    string ComputeHash(string path);
}
