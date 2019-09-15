﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core3Intrinsics
{
    public class AlignedArrayPool<T> : IDisposable where T : struct
    {
        private bool disposedValue = false; // To detect redundant calls

        private static object lockObject = new object();
        private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        private static ArrayPool<T> secondPool = ArrayPool<T>.Shared;
        private const int defaultByteAlignment = 32;

        private readonly int tSize, currentAlignment;        
        private readonly List<(byte[], GCHandle, IntPtr, int)> allBuffers;
        private readonly List<(MemoryHandle, GCHandle, byte[])> allMemoryHandles;
        

        public AlignedArrayPool()
        {            
            Type tp = typeof(T);
            tSize = Marshal.SizeOf(tp);
            if (!tp.IsValueType || tp.IsEnum)
            {
                throw new ArgumentException("Invalid type, must be numeric.");
            }
            currentAlignment = defaultByteAlignment;
            allMemoryHandles = new List<(MemoryHandle, GCHandle, byte[])>();
            allBuffers = new List<(byte[], GCHandle,IntPtr, int)>();
        }

        public ArrayPool<T> Shared => secondPool;
        
        
        public unsafe MemoryHandle Rent(int minimumLength, int byteAlignement)
        {
            byte[] buff = pool.Rent(minimumLength * tSize + byteAlignement);            
            var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            MemoryHandle memHand;
            int currIdx;
            lock (lockObject)
            {
                allBuffers.Add((buff, handle, IntPtr.Zero, 0));
                currIdx = allBuffers.Count - 1;
                IntPtr ptr = AlignBuffer(currIdx);
                memHand = new MemoryHandle(ptr.ToPointer(), handle);
                allMemoryHandles.Add((memHand, handle, buff));
            }
            return memHand;

            unsafe IntPtr AlignBuffer(int bufferIndex)
            {
                (byte[], GCHandle, IntPtr, int) currentBuff = allBuffers[bufferIndex];
                allBuffers.RemoveAt(bufferIndex);
                long lPtr = currentBuff.Item2.AddrOfPinnedObject().ToInt64();
                long lPtr2 = (lPtr + currentAlignment - 1) & ~(currentAlignment - 1);
                currentBuff.Item4 = (int)(lPtr2 - lPtr);
                currentBuff.Item3 = new IntPtr(lPtr2);
                allBuffers.Add(currentBuff);
                return new IntPtr(lPtr2);               
            }
        }

        public MemoryHandle Rent(int minimumLength)
        {
            return Rent(minimumLength, defaultByteAlignment);
        }

        public unsafe void Return(MemoryHandle bufferHandle, bool clearArray = false)
        {
            lock (lockObject)
            {
                (MemoryHandle memHandle, GCHandle gcHandle, byte[] buff) item;
                for (int i = 0; i < allMemoryHandles.Count; i++)
                {
                    item = allMemoryHandles[i];

                    if (item.memHandle.Pointer == bufferHandle.Pointer)
                    {
                        if (item.gcHandle.IsAllocated)
                        {
                            item.gcHandle.Free();
                        }
                        pool.Return(item.buff, clearArray);
                        allMemoryHandles.RemoveAt(i);
                        allBuffers.RemoveAt(i);
                        break;
                    }

                }
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (allMemoryHandles.Count > 0)
                {
                    (MemoryHandle memHandle, GCHandle gcHandle, byte[] buff) item;
                    for (int i = 0; i < allMemoryHandles.Count; i++)
                    {
                        item = allMemoryHandles[i];                        
                        if (item.gcHandle.IsAllocated)
                        {
                            item.gcHandle.Free();
                        }
                        pool.Return(item.buff);
                        
                    }
                    allMemoryHandles.Clear();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~AlignedArrayPool()
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
