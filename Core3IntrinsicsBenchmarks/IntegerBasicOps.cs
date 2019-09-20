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
        [Params(/*4 * 1024,*/ 4000 * 1024)]
        public int NumberOfItems {get; set;}

        private AlignedArrayPool<int> intPool;
        private AlignedArrayPool<short> shortPool;
        private AlignedArrayPool<long> longPool;
        private AlignedMemoryHandle<int> intData, intStore;
        private AlignedMemoryHandle<short> shortData, shortStore;
        private AlignedMemoryHandle<long> longData, longStore;

        [GlobalSetup]
        public void GlobalSetup()
        {
            intPool = new AlignedArrayPool<int>();
            shortPool = new AlignedArrayPool<short>();
            longPool = new AlignedArrayPool<long>();

            intData = intPool.Rent(NumberOfItems);
            intStore = intPool.Rent(NumberOfItems);
            shortData = shortPool.Rent(NumberOfItems);
            shortStore = shortPool.Rent(NumberOfItems);
            longData = longPool.Rent(NumberOfItems);
            longStore = longPool.Rent(NumberOfItems);
            
            var r = new Random(1);
            for (int i = 0; i < NumberOfItems; i++)
            {
                intData.Memory.Span[i] = i * 2 + r.Next(-1000, 1000);
                intStore.Memory.Span[i] = i + r.Next(-1000, 1000);
                shortData.Memory.Span[i] = (short)intData.Memory.Span[i];
                shortStore.Memory.Span[i] = (short)intStore.Memory.Span[i];
                longData.Memory.Span[i] = intData.Memory.Span[i];
                longStore.Memory.Span[i] = intStore.Memory.Span[i];
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            intPool.Return(intData);
            intPool.Return(intStore);
            shortPool.Return(shortData);
            shortPool.Return(shortStore);
            longPool.Return(longData);
            longPool.Return(longStore);
            intPool.Dispose();
        }
        /*
        [BenchmarkCategory("Short"), Benchmark(Baseline = true)]
        public unsafe void ShortAdd()
        {
            var sp1 = new ReadOnlySpan<short>(shortData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<short>(shortStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = (short)(sp1[i] + sp2[i]);
            }
        }

        [BenchmarkCategory("Short"), Benchmark]
        public unsafe void ShortAddVector256()
        {
            ReadOnlySpan<Vector256<short>> sp1 = MemoryMarshal.Cast<short, Vector256<short>>(shortData.Memory.Span);
            Span<Vector256<short>> sp2 = MemoryMarshal.Cast<short, Vector256<short>>(shortStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Add(sp1[i], sp2[i]);
            }
        }

        [BenchmarkCategory("Short"), Benchmark]
        public unsafe void ShortAndNot()
        {
            var sp1 = new ReadOnlySpan<short>(shortData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<short>(shortStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = (short)(sp1[i] & ~sp2[i]);
            }
        }

        [BenchmarkCategory("Short"), Benchmark]
        public unsafe void ShortAndNotVector256()
        {
            ReadOnlySpan<Vector256<short>> sp1 = MemoryMarshal.Cast<short, Vector256<short>>(shortData.Memory.Span);
            Span<Vector256<short>> sp2 = MemoryMarshal.Cast<short, Vector256<short>>(shortStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.AndNot(sp1[i],sp2[i]);
            }
        }

        [BenchmarkCategory("Short"), Benchmark]
        public unsafe void ShortShiftLeft()
        {
            var sp1 = new ReadOnlySpan<short>(shortData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<short>(shortStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = (short)(sp1[i] << 5);
            }
        }

        [BenchmarkCategory("Short"), Benchmark]
        public unsafe void ShortShiftLeftVector256()
        {
            ReadOnlySpan<Vector256<short>> sp1 = MemoryMarshal.Cast<short, Vector256<short>>(shortData.Memory.Span);
            Span<Vector256<short>> sp2 = MemoryMarshal.Cast<short, Vector256<short>>(shortStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.ShiftLeftLogical(sp1[i], 5);
            }
        } */
        /*
        [BenchmarkCategory("Integer"), Benchmark(Baseline = true)]
        public unsafe void IntAdd()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for(int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] + sp2[i];
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntAddVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Add(sp1[i], sp2[i]);
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntXor()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] ^ sp2[i];
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntXorVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Xor(sp1[i], sp2[i]);
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntMultiply()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] * sp2[i];
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntMultiplyLowVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.MultiplyLow(sp1[i], sp2[i]);
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntShiftLeft()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] << 5;
            }
        }


        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntShiftLeftVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.ShiftLeftLogical(sp1[i], 5);
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntMax()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] > sp2[i] ? sp1[1] : sp2[i];
            }
        }

        [BenchmarkCategory("Integer"), Benchmark]
        public unsafe void IntMaxVector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Max(sp1[i], sp2[i]);
            }
        } */

        [BenchmarkCategory("Chained"), Benchmark(Baseline = true)]
        public unsafe void IntMultipleOps()
        {
            var sp1 = new ReadOnlySpan<int>(intData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<int>(intStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = ((sp1[i] > sp2[i] ? sp1[1] : sp2[i]) << 2) * 3;
            }
        }

        [BenchmarkCategory("Chained"), Benchmark]
        public unsafe void IntMultipleOpsvector256()
        {
            ReadOnlySpan<Vector256<int>> sp1 = MemoryMarshal.Cast<int, Vector256<int>>(intData.Memory.Span);
            Span<Vector256<int>> sp2 = MemoryMarshal.Cast<int, Vector256<int>>(intStore.Memory.Span);
            
            Vector256<int> three = Vector256.Create(3);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.MultiplyLow(Avx2.ShiftLeftLogical(Avx2.Max(sp1[i], sp2[i]), 2), three);               
            }
        }

        /*
        [BenchmarkCategory("Long"), Benchmark(Baseline = true)]
        public unsafe void LongAdd()
        {
            var sp1 = new ReadOnlySpan<long>(longData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<long>(longStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] + sp2[i];
            }
        }

        [BenchmarkCategory("Long"), Benchmark]
        public unsafe void LongMultiply()
        {
            var sp1 = new ReadOnlySpan<long>(longData.MemoryHandle.Pointer, NumberOfItems);
            var sp2 = new Span<long>(longStore.MemoryHandle.Pointer, NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                sp2[i] = sp1[i] * sp2[i];
            }
        }

        
        [BenchmarkCategory("Long"), Benchmark]
        public unsafe void LongAddVector256()
        {
            ReadOnlySpan<Vector256<long>> sp1 = MemoryMarshal.Cast<long, Vector256<long>>(longData.Memory.Span);
            Span<Vector256<long>> sp2 = MemoryMarshal.Cast<long, Vector256<long>>(longStore.Memory.Span);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Avx2.Add(sp1[i], sp2[i]);
            }
        }


        [BenchmarkCategory("Long"), Benchmark]
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
        */
    }
}
