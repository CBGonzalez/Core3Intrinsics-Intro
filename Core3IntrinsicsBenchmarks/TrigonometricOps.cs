using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Buffers;

namespace Core3IntrinsicsBenchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
    public class TrigonometricOps
    {
        const int l1CacheSize = 32 * 1024; // one L1 cache, 32 kB
        private int numberOfItems;
        public static int algn = 32;
        public AlignedArrayPool<float> floatPool;
        public AlignedArrayPool<double> doublePool;
        AlignedMemoryHandle<float> dataMemory, resultMemory;
        AlignedMemoryHandle<double> dataDoubleMemory, resultDoubleMemory;

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            numberOfItems = l1CacheSize / sizeof(double) / 2 - 8;
            floatPool = new AlignedArrayPool<float>();
            doublePool = new AlignedArrayPool<double>();
            dataMemory = floatPool.Rent(numberOfItems);
            resultMemory = floatPool.Rent(numberOfItems);
            dataDoubleMemory = doublePool.Rent(numberOfItems);
            resultDoubleMemory = doublePool.Rent(numberOfItems);
            Span<float> dataSpan = new Span<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            Span<float> resultSpan = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);
            Span<double> dataDoubleSpan = new Span<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            Span<double> resultDoubleSpan = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < numberOfItems; i++)
            {
                dataSpan[i] = i + 0.01f;
                resultSpan[i] = 0.0f;
                dataDoubleSpan[i] = i + 0.01;
                resultDoubleSpan[i] = 0.0;
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            floatPool.Return(resultMemory, false);
            floatPool.Return(dataMemory, false);
            doublePool.Return(resultDoubleMemory, false);
            doublePool.Return(dataDoubleMemory, false);
            floatPool.Dispose();
            doublePool.Dispose();
        }

        [Benchmark]
        public unsafe void Cos()
        {
            ReadOnlySpan<float> sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            Span<float> sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = (float)Math.Cos(sp1[i]);
            }
        }

        [Benchmark]
        public unsafe void CosMathF()
        {
            ReadOnlySpan<float> sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            Span<float> sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = MathF.Cos(sp1[i]);
            }
        }

        [Benchmark]
        public unsafe void CosDouble()
        {
            ReadOnlySpan<double> sp1 = new ReadOnlySpan<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            Span<double> sp2 = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Math.Cos(sp1[i]);
                
            }
        }

        

    }
}
