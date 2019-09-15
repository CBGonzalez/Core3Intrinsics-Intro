using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Core3IntrinsicsBenchmarks
{
    public class AlignedMemoryPool<T> : MemoryPool<T> where T : struct
    {
        private const int maxBufferSize = 128 * 1024 * 1024;

        private static readonly AlignedArrayMemoryPool<T> _shared = new AlignedArrayMemoryPool<T>();

        public override int MaxBufferSize => maxBufferSize;

        public static new AlignedMemoryPool<T> Shared => _shared;

        public AlignedMemoryPool()
        {

        }

        public override IMemoryOwner<T> Rent(int minBufferSize = -1)
        {
            return _shared.Rent(minBufferSize);
        }

        public IMemoryOwner<T> Rent(int minBufferSize = -1, int alignment = 32)
        {
            if(minBufferSize > maxBufferSize)
            {
                throw new ArgumentOutOfRangeException($"{nameof(minBufferSize)} must be <= {maxBufferSize}");
            }
            return _shared.Rent(minBufferSize, alignment);
        }


        protected override void Dispose(bool disposing)
        {
            
        }
    }
}
