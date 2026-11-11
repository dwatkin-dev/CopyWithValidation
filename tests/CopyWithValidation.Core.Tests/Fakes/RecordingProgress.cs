namespace CopyWithValidation.Core.Tests.Fakes;

/// <summary>
/// Test listener that records every report in a list, so a test can check them afterwards.
/// Runs immediately on the calling thread (unlike Progress), so tests are reliable.
/// </summary>
public class RecordingProgress<T> : IProgress<T>
{
    // { get; } = read-only property; '= new()' creates the empty list once, up front.
    public List<T> Values { get; } = new();

    public void Report(T value) => Values.Add(value);
}
