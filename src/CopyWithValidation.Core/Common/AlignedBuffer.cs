using System.Runtime.InteropServices;

namespace CopyWithValidation.Core.Common;

internal static class AlignedBuffer
{
    public static Memory<byte> Allocate(int size, int alignment)
    {
        byte[] array = GC.AllocateUninitializedArray<byte>(size + alignment, pinned: true);
        long address = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0).ToInt64();
        int offset = (int)(alignment - address % alignment) % alignment;

        return array.AsMemory(offset, size);
    }
}
