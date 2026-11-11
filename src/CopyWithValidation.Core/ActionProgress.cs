namespace CopyWithValidation.Core;

/// <summary>
/// A progress listener that runs a lambda immediately, on whichever thread calls Report.
/// Used in Core as a small adapter: it takes a report, changes it, and passes it on.
/// </summary>
/// <remarks>
/// Compare with .NET's Progress (used in the window), which POSTS the lambda to the
/// UI thread and runs it later. Never use ActionProgress to touch WPF controls.
/// </remarks>
// <T> = generic type placeholder. ActionProgress<double> means T is double throughout.
// (Action<T> action) = primary constructor: the lambda passed in is stored in 'action'.
// : IProgress<T> = keeps the interface's promise to have a Report(T) method.
public class ActionProgress<T>(Action<T> action) : IProgress<T>
{
    /// <summary>Runs the stored lambda straight away with the reported value.</summary>
    public void Report(T value) => action(value);
}