namespace CopyWithValidation.Core;

public class InsufficientSpaceException(long required, long available)
    : IOException($"Not enough space on the destination: needs {required:N0} bytes, {available:N0} free.")
{
    public long Required { get; } = required;
    public long Available { get; } = available;
}