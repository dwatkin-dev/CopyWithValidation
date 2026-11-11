namespace CopyWithValidation.Core.Tests.Helpers;

/// <summary>
/// Gives each test its own unique source and destination paths in the temp folder,
/// and deletes them afterwards. Use with: using var files = new TempFiles();
/// </summary>
// IDisposable = promises a Dispose() method. 'using var' calls it automatically when the
// test method ends – even if the test fails or throws.
public class TempFiles : IDisposable
{
    // Guid.NewGuid() = random unique name, so tests running at the same time never clash
    public string Source { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    public string Destination { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    /// <summary>Writes <paramref name="size"/> pseudo-random bytes to Source and returns them.</summary>
    public byte[] WriteSource(int size)
    {
        var data = new byte[size];
        new Random(42).NextBytes(data);   // fixed seed 42 = same "random" bytes every run
        File.WriteAllBytes(Source, data);
        return data;
    }

    /// <summary>Writes the text "abc" to Source (its SHA-256 is a published known answer).</summary>
    public string FileContainingAbc()
    {
        File.WriteAllText(Source, "abc");
        return Source;
    }

    public void Dispose()
    {
        // File.Delete doesn't throw if the file isn't there
        File.Delete(Source);
        File.Delete(Destination);
    }
}