using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3IntrinsicsBenchmarks
{
    //[DisassemblyDiagnoser(printAsm: true, printSource: true)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class IntegerBasicOps
    {
        [Params(/*4 * 1024,*/ 400 * 1024)]
        public int NumberOfItems {get; set;}

        private AlignedArrayPool<int> intPool;
        private AlignedArrayPool<short> shortPool;
        private AlignedArrayPool<long> longPool;
        private AlignedMemoryHandle<int> intData, intStore;
        private AlignedMemoryHandle<long> longData, longStore;

        [GlobalSetup]
        public void GlobalSetup()
        {
            intPool = new AlignedArrayPool<int>();
            longPool = new AlignedArrayPool<long>();

            intData = intPool.Rent(NumberOfItems);
            intStore = intPool.Rent(NumberOfItems);
            longData = longPool.Rent(NumberOfItems);
            longStore = longPool.Rent(NumberOfItems);

            Span<int> dataSpan = intData.Memory.Span;
            Span<int> storeSpan = intStore.Memory.Span;
            Span<long> longDataSpan = longData.Memory.Span;
            Span<long> longStoreSpan = longStore.Memory.Span;
            Random r = new Random(1);
            for (int i = 0; i < NumberOfItems; i++)
            {
                dataSpan[i] = i * 2 + r.Next(-1000, 1000);
                storeSpan[i] = i + r.Next(-1000, 1000);
                longDataSpan[i] = dataSpan[i];
                longStoreSpan[i] = storeSpan[i];
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            intPool.Return(intData);
            intPool.Return(intStore);
            intPool.Dispose();
        }

        [Benchmark(Baseline = true)]
        public unsafe void IntAdd()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for(int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] + sp2[i];
            }
        }
        /*
        [Benchmark]
        public unsafe void LongAdd()
        {
            var sp1 = new ReadOnlySpan<long>(longData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<long>(longStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] + sp2[i];
            }
        }
        */
        [Benchmark]
        public unsafe void IntMultiply()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] + sp2[i];
            }
        }

        [Benchmark]
        public unsafe void IntShiftLeft()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] << 5;
            }
        }

        [Benchmark]
        public unsafe void IntMax()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] > sp2[i] ? sp1[1]: sp2[i];
            }
        }

        /*
        [Benchmark]
        public unsafe void LongMultiply()
        {
            var sp1 = new ReadOnlySpan<long>(longData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<long>(longStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] * sp2[i];
            }
        }
        */
        [Benchmark]
        public unsafe void IntAddVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Add(sp1[i], sp2[i]);
            }
        }

        [Benchmark]
        public unsafe void IntShiftLeftVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.ShiftLeftLogical(sp1[i], 5);
            }
        }

        [Benchmark]
        public unsafe void IntMaxVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Min(sp1[i], sp2[i]);
            }
        }

        /*
        [Benchmark]
        public unsafe void LongAddVector256()
        {
            ReadOnlySpan<Vector256<long>> sp1 = MemoryMarshal.Cast<long, Vector256<long>>(longData.Memory.Span);
            Span<Vector256<long>> sp2 = MemoryMarshal.Cast<long, Vector256<long>>(longStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Add(sp1[i], sp2[i]);
            }
        } */


        [Benchmark]
        public unsafe void IntMultiplyVector256ToLong()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            ReadOnlySpan<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);
            Span<Vector256<long>> sp3 = MemoryMarshal.Cast<long, Vector256<long>>(longStore.Memory.Span);
            
            for (int i = 0; i < sp1.Length; i++)
            {
                sp3[i] = Avx2.Multiply(sp1[i], sp2[i]);
            }
        }

    }
}
