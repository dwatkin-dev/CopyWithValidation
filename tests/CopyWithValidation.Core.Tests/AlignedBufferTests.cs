using System.Runtime.InteropServices;
using CopyWithValidation.Core.Common;

namespace CopyWithValidation.Core.Tests;

public class AlignedBufferTests
{
    [Theory]
    [InlineData(1, 4096)]
    [InlineData(4096, 4096)]
    [InlineData(1_048_576, 4096)]
    [InlineData(1000, 512)]
    public void Allocate_ValidSizes_StartsOnAlignedAddress(int size, int alignment)
    {
        Memory<byte> buffer = AlignedBuffer.Allocate(size, alignment);

        long address = AddressOf(buffer);

        Assert.Equal(0L, address % alignment);
    }

    [Theory]
    [InlineData(1, 4096)]
    [InlineData(4096, 4096)]
    [InlineData(1_048_576, 4096)]
    [InlineData(1000, 512)]
    public void Allocate_ValidSizes_HasRequestedLength(int size, int alignment)
    {
        Memory<byte> buffer = AlignedBuffer.Allocate(size, alignment);

        Assert.Equal(size, buffer.Length);
    }

    private static long AddressOf(Memory<byte> buffer)
    {
        MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment);
        return Marshal.UnsafeAddrOfPinnedArrayElement(segment.Array!, segment.Offset).ToInt64();
    }
}