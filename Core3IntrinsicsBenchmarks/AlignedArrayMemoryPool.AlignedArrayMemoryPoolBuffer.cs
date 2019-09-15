using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Core3IntrinsicsBenchmarks
{
    public partial class AlignedArrayMemoryPool<T> where T : struct
    {
        public class AlignedArrayMemoryPoolBuffer : IMemoryOwner<T>
        {                        
            private byte[] byteArray;
            private GCHandle gcHandleToBytes;
            private IntPtr ptrToArray;
            private int offsetToStart, totalTBufferSize;

            const int defaultAlignment = 32;
            private readonly int tSize;

            public IntPtr PtrToArray => ptrToArray;

            private AlignedArrayMemoryPoolBuffer()
            {
                Type t = (new T()).GetType();
                tSize = Marshal.SizeOf(t);
            }

            public AlignedArrayMemoryPoolBuffer(int size) : this(size, defaultAlignment)
            {
                Type t = (new T()).GetType();
                tSize = Marshal.SizeOf(t);
            }

            public AlignedArrayMemoryPoolBuffer(int size, int alignment)
            {
                Type t = (new T()).GetType();
                tSize = Marshal.SizeOf(t);
                byteArray = ArrayPool<byte>.Shared.Rent(size * tSize + 2 * alignment);
                totalTBufferSize = size;
                AlignByteBuffer(alignment);
            }

            public Memory<T> Memory
            {
                get
                {
                    byte[] array = byteArray;
                    if(array == null)
                    {
                        throw new ObjectDisposedException(ToString());
                    }
                    return new Memory<T>(MemoryMarshal.Cast<byte, T>(byteArray.AsSpan(offsetToStart)).ToArray(), 0, totalTBufferSize);
                }
            }

            private void AlignByteBuffer(int alignment)
            {
                gcHandleToBytes = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
                long l1 = gcHandleToBytes.AddrOfPinnedObject().ToInt64();
                long l2 = (l1 + alignment - 1) & ~(alignment - 1);
                // For testing, in order to avoid accidental aligning with 32 bytes
                if(alignment != defaultAlignment && defaultAlignment % alignment == 0)
                {
                    l2 += alignment;
                }
                offsetToStart = (int)(l2 - l1);
                ptrToArray = new IntPtr(l2);
            }

            public void Dispose()
            {
                byte[] retArray = byteArray;
                if (retArray != null)
                {
                    gcHandleToBytes.Free();
                    byteArray = null;
                    ArrayPool<byte>.Shared.Return(retArray);
                }
            }          
        }
    }
}
