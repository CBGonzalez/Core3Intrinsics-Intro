using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;
using System.Buffers;

namespace Core3Intrinsics
{
    public class Loading :IDisposable
    {
        const int numberOfItems = 1024;
        private static int algn = 32;
        public static AlignedMemory alignedBuffer, alignedStoreBuffer, alignedStoreBuffer2;
        public static AlignedMemoryDouble alignedDouble, alignedStoreDouble;
        public static AlignedMemoryGeneric<float> genericAlignedFloat, genericAlignedStoreFloat, genericAligned16Float;
        AlignedArrayPool<float> floatArrayPool;
        public static MemoryHandle dataMemHandle, resultMemHandle;
        AlignedMemoryPool<float> alPool;
        public IMemoryOwner<float> dataMem, storeMem, store2Mem;
        //public static AlignedMemoryManager<float> memManFloat;        
        
        //Memory<float> floatBufferMem;
        //Memory<Vector256<float>> floatVectorsMem;

        public Loading() : this(algn)
        {

        }

        public unsafe Loading(int alignmentBytes)
        {
            algn = alignmentBytes;
            //float[] testArray = new float[128];
            //for (int i = 0; i < testArray.Length; i++)
            //{
            //    testArray[i] = i + 1;
            //}
            //testingAligned = new AlignedMemoryT<float>(testArray, 32);
            alPool = AlignedMemoryPool<float>.Shared;

            dataMem = alPool.Rent(numberOfItems, algn);
            storeMem = alPool.Rent(numberOfItems, algn);
            store2Mem = alPool.Rent(numberOfItems, algn);

            alignedBuffer = new AlignedMemory(numberOfItems, algn);
            alignedStoreBuffer = new AlignedMemory(numberOfItems, algn);
            alignedStoreBuffer2 = new AlignedMemory(numberOfItems, algn);
            alignedDouble = new AlignedMemoryDouble(numberOfItems, algn);
            alignedStoreDouble = new AlignedMemoryDouble(numberOfItems, algn);
            genericAlignedFloat = new AlignedMemoryGeneric<float>(numberOfItems, algn);
            genericAligned16Float = new AlignedMemoryGeneric<float>(numberOfItems, 16);
            genericAlignedStoreFloat = new AlignedMemoryGeneric<float>(numberOfItems);
            float[] backingFloatArray = new float[numberOfItems];
            floatArrayPool = new AlignedArrayPool<float>();
            dataMemHandle = floatArrayPool.Rent(numberOfItems);
            resultMemHandle = floatArrayPool.Rent(numberOfItems);
            Span<float> dataMemHandSpan = new Span<float>(dataMemHandle.Pointer, numberOfItems);
            Span<float> resultMemHandSpan = new Span<float>(resultMemHandle.Pointer, numberOfItems);
            
            float* spPtr = (float*)alignedBuffer.GetPointer().ToPointer();
            float* spPtr2 = (float*)alignedStoreBuffer.GetPointer().ToPointer();
            float* spPtr3 = (float*)alignedStoreBuffer2.GetPointer().ToPointer();
            float* spPtr4 = (float*)genericAlignedFloat.BufferIntPtr.ToPointer();
            float* spPtr5 = (float*)genericAlignedStoreFloat.BufferIntPtr.ToPointer();
            //double* dPtr = (double*)alignedDouble.GetPointer().ToPointer();            
            int step = sizeof(float);
            int counter = 1;
            for (int i = 0; i < alignedBuffer.ByteLength; i += step)
            {
                *spPtr = counter;                
                *spPtr4 = counter;
                *spPtr2 = 0;
                *spPtr3 = 0;
                *spPtr5 = 0;

                spPtr++;               
                spPtr2++;
                spPtr3++;
                spPtr4++;
                spPtr5++;
                counter++;
            }
            counter = 1;
            for(int i = 0; i < alignedDouble.TLength; i++)
            {
                alignedDouble[i] = counter;
                alignedStoreDouble[i] = 0;
                counter++;
            }
            counter = 1;
            for (int i = 0; i < backingFloatArray.Length; i++)
            {
                backingFloatArray[i] = counter;                
                counter++;
            }
            counter = 1;
            for(int i = 0; i < dataMemHandSpan.Length; i++)
            {
                dataMemHandSpan[i] = counter;
                resultMemHandSpan[i] = 0;
                counter++;
            }
        }

        /* public unsafe bool TestAlignement()
        {
            bool result = true;
            try
            {
                Vector256<float> testVec = Avx.LoadAlignedVector256(((float*)genericAligned16Float.BufferIntPtr.ToPointer()));
            }
            catch (System.AccessViolationException)
            {
                result = false;
            }
            return result;
        } */
        public unsafe void SquareRoot()
        {
            //ReadOnlySpan<float> sp1 = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(genericAlignedFloat.BufferIntPtr.ToPointer(), genericAlignedFloat.ByteLength));
            //Span<float> sp2 = MemoryMarshal.Cast<byte, float>(new Span<byte>(genericAlignedStoreFloat.BufferIntPtr.ToPointer(), genericAlignedStoreFloat.ByteLength));
            ReadOnlySpan<float> sp1 = new ReadOnlySpan<float>(dataMemHandle.Pointer, numberOfItems);
            Span<float> sp2 = new Span<float>(resultMemHandle.Pointer, numberOfItems);
            //ReadOnlySpan<float> sp1 = dataMem.Memory.Span;
            //Span<float> sp2 = store2Mem.Memory.Span;

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = (float)Math.Sqrt(sp1[i]);
            }
        }

        public unsafe void VectorSquareRoot()
        {
            ReadOnlySpan<float> sp1 = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(alignedBuffer.GetPointer().ToPointer(), alignedBuffer.ByteLength));
            ReadOnlySpan<Vector256<float>> vecSpan = MemoryMarshal.Cast<float, Vector256<float>>(sp1);
            
            int step = Vector256<float>.Count;
            float* currSpPtr = (float*)alignedBuffer.GetPointer().ToPointer();
            float* currSpPtr2 = (float*)alignedStoreBuffer2.GetPointer().ToPointer();

            for (int i = 0; i < vecSpan.Length; i++)
            {
                Avx.StoreAligned(currSpPtr2, Avx.Sqrt(Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }

        public unsafe void RVectorSquareRoot()
        {
            var floSpan = new Span<float>(alignedBuffer.GetPointer().ToPointer(), alignedBuffer.TLength);
            ReadOnlySpan<Vector256<float>> flSpan = MemoryMarshal.Cast<float, Vector256<float>>(floSpan);

            int step = Vector256<float>.Count;
            float* currSpPtr = (float*)alignedBuffer.GetPointer().ToPointer();
            float* currSpPtr2 = (float*)alignedStoreBuffer.GetPointer().ToPointer();

            for (int i = 0; i < flSpan.Length; i++)
            {
                Avx.StoreAligned(currSpPtr2, Avx.ReciprocalSqrt(Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }


        public unsafe void RVectorSquareRootDouble()
        {
            double one = 1.0;
            double* onePtr = &one;
            Vector256<double> oneVec = Avx.BroadcastScalarToVector256(onePtr);
            
            var douSpan = new Span<double>(alignedDouble.GetIntPointer().ToPointer(), alignedDouble.TLength);
            ReadOnlySpan<Vector256<double>> doSpan = MemoryMarshal.Cast<double, Vector256<double>>(douSpan);
            
            int step = Vector256<double>.Count;
            double* currSpPtr = (double*)alignedDouble.GetIntPointer().ToPointer();
            double* currSpPtr2 = (double*)alignedStoreDouble.GetIntPointer().ToPointer();

            for (int i = 0; i < doSpan.Length; i++)
            {

                Avx.StoreAligned(currSpPtr2, Avx.Divide(oneVec, Avx.Sqrt(Avx.LoadAlignedVector256(currSpPtr))));
                currSpPtr += step;
                currSpPtr2 += step;
            }                                            
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
                    alignedBuffer.Dispose();
                    alignedDouble.Dispose();
                    alignedStoreBuffer.Dispose();
                    alignedStoreBuffer2.Dispose();
                    alignedStoreDouble.Dispose();
                    genericAlignedFloat.Dispose();
                    genericAlignedStoreFloat.Dispose();
                    floatArrayPool.Dispose();
                    dataMem.Dispose();
                    storeMem.Dispose();
                    store2Mem.Dispose();
                    //memManFloat.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Loading()
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
        /*
        public unsafe void VectorStore()
        {
            Span<float> floSpan = new Span<float>(alignedBuffer.GetPointer().ToPointer(), alignedBuffer.TLength);
            ReadOnlySpan<Vector256<float>> flSpan = MemoryMarshal.Cast<float, Vector256<float>>(floSpan);
            Span<float> floSpan2 = new Span<float>(alignedStoreBuffer.GetPointer().ToPointer(), alignedStoreBuffer.TLength);
            Span<Vector256<float>> flSpan2 = MemoryMarshal.Cast<float, Vector256<float>>(floSpan2);
            //fixed (float* spPtr = &floatBufferMem.Span[0])
            //{
            int step = Vector256<float>.Count;
            float* currSpPtr = (float*)alignedBuffer.GetPointer().ToPointer();

            for (int i = 0; i < flSpan.Length; i++)
            {
                flSpan2[i] = Avx.LoadAlignedVector256(currSpPtr);
            }
            currSpPtr += step;
        }*/

    }            
}
