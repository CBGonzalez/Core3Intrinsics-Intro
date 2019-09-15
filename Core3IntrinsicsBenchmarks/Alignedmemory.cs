using System;
using System.Runtime.InteropServices;

namespace Core3IntrinsicsBenchmarks
{
    public class AlignedMemoryFloat: IDisposable
    {
        //private Memory<T> mem;
        public  byte[] Buffer;
        public readonly int TLength, ByteLength;
        private GCHandle bufferHandle;
        private readonly IntPtr bufferPtr;
        private readonly int tSize;        

        public unsafe AlignedMemoryFloat(int howManyTs, int alignmentInBytes)
        {            
            tSize = sizeof(float);
            Buffer = new byte[(howManyTs * tSize) + alignmentInBytes];
            bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            long lPtr = bufferHandle.AddrOfPinnedObject().ToInt64();
            long lPtr2 = (lPtr + alignmentInBytes - 1) & ~(alignmentInBytes - 1);
            bufferPtr = new IntPtr(lPtr2);
            //Span<byte> sp = new Span<byte>(Buffer, (int)(lPtr2 - lPtr), howManyTs * tSize);
            
            //Span<T> spT = MemoryMarshal.Cast<byte, T>(sp);
            TLength = howManyTs;
            ByteLength = TLength * tSize;
        }

        public IntPtr GetPointer()
        {
            return bufferPtr;
        }

        public float GetElement(int x)
        {
            return BitConverter.ToSingle(Buffer, x * tSize);
        }

        public void StoreElement(float value, int position)
        {
            byte[] valBytes = BitConverter.GetBytes(value);
            Array.Copy(valBytes, 0, Buffer, position * tSize, valBytes.Length);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(bufferHandle.IsAllocated)
                    {
                        bufferHandle.Free();
                        Buffer = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AlignedMemory()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
