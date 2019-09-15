using System;
using System.Runtime.InteropServices;

namespace Core3IntrinsicsBenchmarks
{
    public class AlignedMemoryGeneric<T>:IDisposable where T:struct
    {
        private const int defaultAlignment = 32; //32 byte alignment needed for 256 bit vectors, 16 for 128 bit

        private byte[] rawBuffer;
        private T[] tBuffer;
        private GCHandle bufferHandle;
        private  IntPtr bufferIntPtr;
        private readonly int tSize;        
        private readonly int currentAlignment;
        private Memory<T> memory;
        private readonly int tLength;
        private readonly int byteLength;
        private int beginOffset;

        public int TLength => tLength;
        public int ByteLength => byteLength;
        public int BeginOffset => beginOffset;
        public IntPtr BufferIntPtr => bufferIntPtr;
        public Memory<T> Memory => memory;

        public T[] TBuffer => tBuffer;

        public byte[] ByteBuffer => rawBuffer;
        
        public AlignedMemoryGeneric(int howManyTs, int byteAlignement)
        {
            Type tType = typeof(T);
            currentAlignment = byteAlignement;
            if(!tType.IsValueType || tType.IsEnum)
            {
                throw new ArgumentException("Invalid type, must be numeric.");
            }
            tSize = Marshal.SizeOf(new T());
            tLength = howManyTs;
            byteLength = tLength * tSize;
            rawBuffer = new byte[byteLength + (2 * currentAlignment)]; //Allow space for aligned memory; could be 1 *, but see below
            CreateAlignedPinnedMemory();            
        }

        public AlignedMemoryGeneric(int howManyTs) : this(howManyTs, defaultAlignment)
        {

        }

        private unsafe void CreateAlignedPinnedMemory()
        {
            bufferHandle = GCHandle.Alloc(rawBuffer, GCHandleType.Pinned);
            long lPtr = bufferHandle.AddrOfPinnedObject().ToInt64();
            long lPtr2 = (lPtr + currentAlignment - 1) & ~(currentAlignment - 1);
            if(currentAlignment != defaultAlignment && currentAlignment % defaultAlignment == 0) // force away from 32 byte aligment for benchmarking purposes
            {
                lPtr2 += currentAlignment;
            }
            bufferIntPtr = new IntPtr(lPtr2);
            beginOffset = (int)(lPtr2 - lPtr);
            tBuffer = MemoryMarshal.Cast<byte, T>(new Span<byte>(bufferIntPtr.ToPointer(), length: byteLength)).ToArray();
            memory = new Memory<T>(tBuffer);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                if (bufferHandle.IsAllocated)
                {
                    bufferHandle.Free();
                }
                rawBuffer = null;
                disposedValue = true;
            }
        }
        
        ~AlignedMemoryGeneric()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
