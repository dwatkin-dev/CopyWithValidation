namespace CopyWithValidation.Core.Tests.Helpers;

public class TempFiles : IDisposable
{
    public string Source { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    public string Destination { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public byte[] WriteSource(int size)
    {
        var data = new byte[size];
        new Random(42).NextBytes(data);
        File.WriteAllBytes(Source, data);
        return data;
    }

    public void Dispose()
    {
        File.Delete(Source);
        File.Delete(Destination);
    }
}