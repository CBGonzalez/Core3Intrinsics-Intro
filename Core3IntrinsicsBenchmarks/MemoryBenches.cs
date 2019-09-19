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
    //[Config(typeof(Config))] // only used for plots
    public class MemoryBenches
    {
        private class Config : ManualConfig // only used for plots
        {
            public Config()
            {
                Add(CsvMeasurementsExporter.Default);
                Add(RPlotExporter.Default);
            }
        }
       
        [Params(16 * 1024, 128 * 1024, 1024 * 1024, 2 * 1024 * 1024, 8 * 1024 * 1024)] // half L1, half L2, half L3, 2 * L3
        public int NumberOfBytes { get ; set; }

        private int vectorNumberOfItems, vectorFloatStep;        
        private int numberOfFloatItems;

        private static readonly AlignedArrayPool<float> alignedArrayPool = new AlignedArrayPool<float>();
        private static AlignedMemoryHandle<float> dataMemory, storeMemory, data16Memory, store16Memory;
        //private static float[] arr1, arr2;

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {            
            vectorFloatStep = Vector256<float>.Count;
            numberOfFloatItems = NumberOfBytes / sizeof(float);
            vectorNumberOfItems = numberOfFloatItems / vectorFloatStep;
            
            dataMemory = alignedArrayPool.Rent(numberOfFloatItems);
            storeMemory = alignedArrayPool.Rent(numberOfFloatItems);
            data16Memory = alignedArrayPool.Rent(numberOfFloatItems, 16);
            store16Memory = alignedArrayPool.Rent(numberOfFloatItems, 16);

            for (int i = 0; i < numberOfFloatItems; i++)
            {
                dataMemory.Memory.Span[i] = i;
                data16Memory.Memory.Span[i] = i;
            }            
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            alignedArrayPool.Return(dataMemory);
            alignedArrayPool.Return(storeMemory);
            alignedArrayPool.Return(data16Memory);
            alignedArrayPool.Return(store16Memory);
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

            int step = 4;
            for (int i = 0; i < dataAl.Length; i += step)
            {
                storeAl[i] = dataAl[i];
                storeAl[i + 1] = dataAl[i + 1];
                storeAl[i + 2] = dataAl[i + 2];
                storeAl[i + 3] = dataAl[i + 3];                
            }
        }

        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void PtrCopyUnrolled()
        {
            float* arr1Ptr = (float*)data16Memory.MemoryHandle.Pointer;
            float* arr2Ptr = (float*)store16Memory.MemoryHandle.Pointer;

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
                
            
        }  */

        [BenchmarkCategory("Aligned Memory"), Benchmark]
        public void ScalarCopyBlock()
        {
            Unsafe.CopyBlock(ref storeMemory.ByteRef, ref dataMemory.ByteRef, (uint)(numberOfFloatItems * sizeof(float)));             
        }

        
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
        }

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
        }

        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public void VectorArraySafeUnaligned()
        {            
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(data16Memory.Memory.Span);
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(store16Memory.Memory.Span);

            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        } */

        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorStoreUnalignedUnsafe()
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

        [BenchmarkCategory("Unaligned Memory"), Benchmark]
        public unsafe void VectorStoreUnalignedToAlignedUnsafe()
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
        }
    }
}
