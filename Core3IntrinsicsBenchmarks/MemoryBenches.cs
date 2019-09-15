using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3IntrinsicsBenchmarks
{
    //[DisassemblyDiagnoser(printAsm: true, printSource: true)]
    //[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    //[CategoriesColumn]
    //[Config(typeof(Config))]
    public class MemoryBenches
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(CsvMeasurementsExporter.Default);
                Add(RPlotExporter.Default);
            }
        }

        //const int numberOfItems = 4 * 1024 * 1024;// L3 cache size //16 * 1024; // half of one L1 cache size
        [Params(16 * 1024, 128 * 1024, 1024 * 1024, 2 * 1024 * 1024, 8 * 1024 * 1024)] // half L1, half L2, half L3, 2 * L3
        public int NumberOfBytes { get ; set; }
        private int vectorNumberOfItems, vectorFloatStep;
        public static int algn = 32;
        int numberOfFloatItems;

        public AlignedArrayPool<float> alignedArrayPool = new AlignedArrayPool<float>();//, aligned16Store;
        AlignedMemoryPool<float> pool;
        AlignedArrayMemoryPool<float>.AlignedArrayMemoryPoolBuffer aligned32Data1, aligned32Data2;//,  aligned16Data1, aligned16Data2;
        Memory<float> aligned32MemPool1, aligned32MemPool2;
        AlignedMemoryHandle<float> dataMemory, storeMemory, data16Memory, store16Memory;
        //AlignedMemoryGeneric<float> floatAligned32Data1, floatAligned32Data2, floatAligned16Data1, floatAligned16Data2;
        private static float[] arr1, arr2;

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {            
            vectorFloatStep = Vector256<float>.Count;
            numberOfFloatItems = NumberOfBytes / sizeof(float);
            vectorNumberOfItems = numberOfFloatItems / vectorFloatStep;
            pool = AlignedMemoryPool<float>.Shared;
            aligned32Data1 = (AlignedArrayMemoryPool<float>.AlignedArrayMemoryPoolBuffer)pool.Rent(numberOfFloatItems, 32);// new AlignedArrayPool<float>();
            aligned32MemPool1 = aligned32Data1.Memory;

            aligned32Data2 = (AlignedArrayMemoryPool<float>.AlignedArrayMemoryPoolBuffer)pool.Rent(numberOfFloatItems, 32);// new AlignedArrayPool<float>();
            aligned32MemPool2 = aligned32Data2.Memory;
            dataMemory = alignedArrayPool.Rent(numberOfFloatItems);
            storeMemory = alignedArrayPool.Rent(numberOfFloatItems);
            data16Memory = alignedArrayPool.Rent(numberOfFloatItems, 16);
            store16Memory = alignedArrayPool.Rent(numberOfFloatItems, 16);

            //floatAligned32Data1 = new AlignedMemoryGeneric<float>(numberOfFloatItems);
            //floatAligned32Data2 = new AlignedMemoryGeneric<float>(numberOfFloatItems);
            //floatAligned16Data1 = new AlignedMemoryGeneric<float>(numberOfFloatItems, 16);
            //floatAligned16Data2 = new AlignedMemoryGeneric<float>(numberOfFloatItems, 16);

            //arr1 = new float[numberOfFloatItems];
            //arr2 = new float[numberOfFloatItems];
            arr1 = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            arr2 = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            //aligned16Data1 = (AlignedArrayMemoryPool<float>.AlignedArrayMemoryPoolBuffer)pool.Rent(numberOfFloatItems, 16); //new AlignedArrayPool<float>(16);
            //aligned16Data2 = (AlignedArrayMemoryPool<float>.AlignedArrayMemoryPoolBuffer)pool.Rent(numberOfFloatItems, 16); //new AlignedArrayPool<float>(16);

            //dataMemory = aligned32Data.Rent(numberOfFloatItems);
            //storeMemory = aligned32Data.Rent(numberOfFloatItems);
            //data16Memory = aligned16Store.Rent(numberOfFloatItems);
            //store16Memory = aligned16Store.Rent(numberOfFloatItems);

            //var dataAl = new Span<float>(dataMemory.Pointer, numberOfFloatItems);
            //var dataAl16 = new Span<float>(data16Memory.Pointer, numberOfFloatItems);

            for (int i = 0; i < numberOfFloatItems; i++)
            {
                aligned32MemPool1.Span[i] = i;
                arr1[i] = i;
                //floatAligned32Data1.Memory.Span[i] = i;
                //floatAligned16Data1.Memory.Span[i] = i;
                //aligned16Data1.Memory.Span[i] = i;
            }
            //for (int i = 0; i < numberOfFloatItems; i++)
            //{                
            //    dataAl[i] = i;
            //    dataAl16[i] = i;                
            //}
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            //aligned32Data.Return(dataMemory, false);
            //aligned32Data.Return(storeMemory, false);
            //aligned16Store.Return(data16Memory, false);
            //aligned16Store.Return(store16Memory, false);
            //aligned16Data1.Dispose();
            //aligned16Data2.Dispose();
            //floatAligned16Data1.Dispose();
            //floatAligned16Data2.Dispose();
            //floatAligned32Data1.Dispose();
            //floatAligned32Data2.Dispose();
            aligned32Data1.Dispose();
            aligned32Data2.Dispose();

            alignedArrayPool.Return(dataMemory);
            alignedArrayPool.Return(storeMemory);
            alignedArrayPool.Return(data16Memory);
            alignedArrayPool.Return(store16Memory);
            ArrayPool<float>.Shared.Return(arr1);
            ArrayPool<float>.Shared.Return(arr2);
        }

        /*
        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public unsafe void ScalarStore()
        {            
            ReadOnlySpan<float> dataAl = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(dataMemory.MemoryHandle.Pointer, dataMemory.ByteArrayLength));
            Span<float> storeAl = MemoryMarshal.Cast<byte, float>(new Span<byte>(storeMemory.MemoryHandle.Pointer, storeMemory.ByteArrayLength));
            for (int i = 0; i < dataAl.Length; i++)
            {
                storeAl[i] = dataAl[i];
            }
        } 
        
        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public unsafe void ScalarStoreUnrolled()
        {
            ReadOnlySpan<float> dataAl = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(dataMemory.MemoryHandle.Pointer, dataMemory.ByteArrayLength));
            Span<float> storeAl = MemoryMarshal.Cast<byte, float>(new Span<byte>(storeMemory.MemoryHandle.Pointer, storeMemory.ByteArrayLength));
            //ReadOnlySpan<float> dataAl = aligned32Mem1.Span;
            //Span<float> storeAl = aligned32Mem2.Span;
            int step = 4;
            for (int i = 0; i < dataAl.Length; i += step)
            {
                storeAl[i] = dataAl[i];
                storeAl[i + 1] = dataAl[i + 1];
                storeAl[i + 2] = dataAl[i + 2];
                storeAl[i + 3] = dataAl[i + 3];                
            }
        }


        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public unsafe void PtrCopyUnrolled()
        {
            fixed(float* pt1 = &arr1[0])
            {
                fixed(float* pt2 = &arr2[0])
                {
                    float* arr1Ptr = pt1;
                    float* arr2Ptr = pt2;
                    int i = 0;
                    while (i < numberOfFloatItems)
                    {
                        *arr2Ptr = *arr1Ptr;
                        arr1Ptr++;
                        arr2Ptr++;
                        *arr2Ptr = *arr1Ptr;
                        arr1Ptr++;
                        arr2Ptr++;
                        *arr2Ptr = *arr1Ptr;
                        arr1Ptr++;
                        arr2Ptr++;
                        *arr2Ptr = *arr1Ptr;
                        arr1Ptr++;
                        arr2Ptr++;
                        
                        i += 4;
                    }
                }
            }
        }
        */

        /*
    [BenchmarkCategory("Aligned Memory"), Benchmark(Baseline = true)]
    public void ScalarCopyBlock()
    {
        Unsafe.CopyBlock(ref storeMemory.ByteRef, ref dataMemory.ByteRef, (uint)(numberOfFloatItems * sizeof(float)));             
    } */

        
        [BenchmarkCategory("Aligned Memory"), Benchmark(Baseline = true)]
        
        public unsafe void VectorStoreAlignedUnsafe()
        {
            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)storeMemory.MemoryHandle.Pointer;

            int i = 0;
            while (i < vectorNumberOfItems)
            {
                Avx.StoreAligned(currSpPtr2, Avx.LoadAlignedVector256(currSpPtr));
                currSpPtr += vectorFloatStep;
                currSpPtr2 += vectorFloatStep;
                i++;
            }
        }
        

        //[BenchmarkCategory("Aligned Memory"), Benchmark(Baseline = true)]
        //public unsafe void VectorStoreAligned2()
        //{
        //    float* currSpPtr = (float*)floatAligned32Data1.BufferIntPtr.ToPointer();
        //    float* currSpPtr2 = (float*)floatAligned32Data2.BufferIntPtr.ToPointer();
        //
        //    int i = 0;
        //    while (i < vectorNumberOfItems)
        //    {
        //        Avx.StoreAligned(currSpPtr2, Avx.LoadAlignedVector256(currSpPtr));
        //        currSpPtr += vectorFloatStep;
        //        currSpPtr2 += vectorFloatStep;
        //        i++;
        //    }
        //}
        /*
        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public unsafe void VectorStoreArrayMemPtr()
        {            
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfFloatItems));
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(storeMemory.MemoryHandle.Pointer, numberOfFloatItems));

            int i = 0;
            
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];                
                i++;
            }
        }
        
        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public void VectorStoreArrayMemSafe()
        {
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(dataMemory.Memory.Span);
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(storeMemory.Memory.Span);

            int i = 0;

            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        } */

        //[BenchmarkCategory("Aligned Memory"), Benchmark]
        //public unsafe void VectorStoreArrayMemPtr2()
        //{
        //    ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(floatAligned32Data1.Memory.Span);
        //    Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(floatAligned32Data2.Memory.Span);
        //
        //    int i = 0;
        //
        //    while (i < readMem.Length)
        //    {
        //        writeMem[i] = readMem[i];
        //        i++;
        //    }
        //}
        /*
        [BenchmarkCategory("Unaligned Memory"), Benchmark]        
        public unsafe void VectorStoreArrayRentedBuffer()
        {
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(arr1);
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(arr2);

            int i = 0;

            while (i < vectorNumberOfItems)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        } */

        /*
        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorStoreArrayMemPtrUnaligned()
        {
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(data16Memory.MemoryHandle.Pointer, numberOfFloatItems));
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(store16Memory.MemoryHandle.Pointer, numberOfFloatItems));
            
            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        } */
       
        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public void VectorArraySafe()
        {            
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(data16Memory.Memory.Span);
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(store16Memory.Memory.Span);

            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        }

        
        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorStoreUnsafe()
        {            
            float* currSpPtr = (float*)data16Memory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)store16Memory.MemoryHandle.Pointer;

            int i = 0;
            while (i < vectorNumberOfItems)
            {
                Avx.Store(currSpPtr2, Avx.LoadVector256(currSpPtr));
                currSpPtr += vectorFloatStep;
                currSpPtr2 += vectorFloatStep;
                i++;
            }
        }

        //[BenchmarkCategory("Unaligned Memory"), Benchmark]
        //public unsafe void VectorStoreArrayMemPtrUnaligned2()
        //{
        //    ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(floatAligned16Data1.Memory.Span);
        //    Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(floatAligned16Data2.Memory.Span);
        //
        //    int i = 0;
        //
        //    while (i < readMem.Length)
        //    {
        //        writeMem[i] = readMem[i];
        //        i++;
        //    }
        //}
        /*
        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorArrayUnalignedSafe()
        {
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(data16Memory.MemoryHandle.Pointer, numberOfFloatItems));
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(store16Memory.MemoryHandle.Pointer, numberOfFloatItems));

            //ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(MemoryMarshal.Cast<byte, float>(aligned32Pool.GetByteMemory(dataMemory).Span));
            //Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(MemoryMarshal.Cast<byte, float>(aligned32Pool.GetByteMemory(storeMemory).Span));
            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        }
        */
        /*
        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorStoreUnalignedToAligned()
        {            
            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)storeMemory.MemoryHandle.Pointer;

            int i = 0;
            while (i < vectorNumberOfItems)
            {
                Avx.Store(currSpPtr2, Avx.LoadVector256(currSpPtr));
                currSpPtr += vectorFloatStep;
                currSpPtr2 += vectorFloatStep;
                i++;
            }
        }  */ 
    }
}
