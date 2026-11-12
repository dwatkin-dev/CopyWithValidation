namespace CopyWithValidation.Core.Tests.Fakes;

public class RecordingProgress<T> : IProgress<T>
{
    public List<T> Values { get; } = new();

    public void Report(T value) => Values.Add(value);
}
