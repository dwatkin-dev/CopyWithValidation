namespace CopyWithValidation.Core.Tests.Fakes;

public class MismatchingFakeHasher : IFileHasher
{
    private int _calls;

    public string ComputeHash(string path)
    {
        _calls++;
        return _calls.ToString();
    }
}
