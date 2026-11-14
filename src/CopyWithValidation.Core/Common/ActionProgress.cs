namespace CopyWithValidation.Core.Common;

public class ActionProgress<T>(Action<T> action) : IProgress<T>
{
    public void Report(T value) => action(value);
}