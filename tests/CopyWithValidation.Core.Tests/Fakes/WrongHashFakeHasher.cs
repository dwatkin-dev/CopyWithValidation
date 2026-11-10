namespace CopyWithValidation.Core.Tests.Fakes;

public class WrongHashFakeHasher : IFileHasher
{
    public string ComputeHash(string path) => "not-a-real-hash";
}
