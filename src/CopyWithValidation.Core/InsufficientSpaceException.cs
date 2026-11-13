namespace CopyWithValidation.Core;

public class InsufficientSpaceException(long required, long available)
    : IOException($"Not enough space on the destination: needs {ByteSize.Format(required)}, {ByteSize.Format(available)} free.")
{
    public long Required { get; } = required;
    public long Available { get; } = available;
}