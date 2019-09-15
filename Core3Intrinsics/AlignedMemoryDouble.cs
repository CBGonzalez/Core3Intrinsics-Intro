using System;
using System.Runtime.InteropServices;


namespace Core3Intrinsics
{
    public class AlignedMemoryDouble
    {
        public byte[] Buffer;
        public readonly int TLength, ByteLength;
        public int BeginOffset;
        private GCHandle bufferHandle;
        private readonly IntPtr bufferPtr;
        private readonly int doubleSize;

        public unsafe AlignedMemoryDouble(int howManyDoubles, int alignmentInBytes)
        {
            doubleSize = sizeof(double);
            Buffer = new byte[(howManyDoubles * doubleSize) + alignmentInBytes];
            bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            long lPtr = bufferHandle.AddrOfPinnedObject().ToInt64();
            long lPtr2 = (lPtr + alignmentInBytes - 1) & ~(alignmentInBytes - 1);
            bufferPtr = new IntPtr(lPtr2);
            BeginOffset = (int)(lPtr2 - lPtr);            
            TLength = howManyDoubles;
            ByteLength = TLength * doubleSize;
        }

        public IntPtr GetIntPointer()
        {
            return bufferPtr;
        }
        
        public double this[int index]
        {            
            get
            {
                unsafe { return GetPointer()[index]; }
            }
            set
            {
                unsafe
                {
                    GetPointer()[index] = value;
                }
            }
        }

        public unsafe double* GetPointer(int index)
        {
            if(index >= TLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return GetPointer() + index;
        }

        public unsafe double* GetPointer()
        {
            return ((double*)bufferPtr.ToPointer());
        }

        public double GetElement(int x)
        {
            return BitConverter.ToDouble(Buffer, x * doubleSize);
        }

        public void StoreElement(double value, int position)
        {
            byte[] valBytes = BitConverter.GetBytes(value);
            Array.Copy(valBytes, 0, Buffer, position * doubleSize, valBytes.Length);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (bufferHandle.IsAllocated)
                {
                    bufferHandle.Free();
                    Buffer = null;
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~AlignedMemoryDouble()
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
