using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core3IntrinsicsBenchmarks
{
    public partial class AlignedArrayMemoryPool<T> : AlignedMemoryPool<T> where T : struct
    {
        private const int MaximumBufferSize = int.MaxValue;

        public override int MaxBufferSize => MaximumBufferSize;
        
        public new IMemoryOwner<T> Rent(int minimumBufferSize = -1, int byteAlignment = 32)
        {
            if (minimumBufferSize == -1)
            {
                minimumBufferSize = 1 + (4095 / Unsafe.SizeOf<T>());
            }
            else if (((uint)minimumBufferSize) > MaximumBufferSize)
            {
                throw new ArgumentOutOfRangeException($"{nameof(minimumBufferSize)} must be <= {MaximumBufferSize}");
            }

            return new AlignedArrayMemoryPoolBuffer(minimumBufferSize, byteAlignment);
        }

        public override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
        {
            return Rent(minimumBufferSize, 32);
        }
    }
}
