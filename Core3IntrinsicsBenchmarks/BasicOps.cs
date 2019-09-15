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
    public class BasicOps
    {
        [Params(/*32 * 1024, 256 * 1024,*/ 4 * 1024 * 1024)]
        public int ParamCacheSize { get; set; }

        //private const int cacheSize = 32 * 1024; // one L1 cache, 32 kB
        private int numberOfItems;
        public static int algn = 32;       
        public AlignedArrayPool<float> floatPool;
        public AlignedArrayPool<double> doublePool;
        AlignedMemoryHandle<float> dataMemory, dataMemory2, dataMemory3, resultMemory;//
        AlignedMemoryHandle<double> dataDoubleMemory, resultDoubleMemory;
        Memory<float> data, data2, data3, result;         

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            //numberOfItems = cacheSize / sizeof(double) / 2 - 8;
            numberOfItems = ParamCacheSize / sizeof(double) / 2 - 8;
            floatPool = new AlignedArrayPool<float>();
            doublePool = new AlignedArrayPool<double>();
            dataMemory = floatPool.Rent(numberOfItems);
            dataMemory2 = floatPool.Rent(numberOfItems);
            dataMemory3 = floatPool.Rent(numberOfItems);
            resultMemory = floatPool.Rent(numberOfItems);
            dataDoubleMemory = doublePool.Rent(numberOfItems);
            resultDoubleMemory = doublePool.Rent(numberOfItems);
            data = new Memory<float>(new float[numberOfItems]);
            data2 = new Memory<float>(new float[numberOfItems]);
            data3 = new Memory<float>(new float[numberOfItems]);
            result = new Memory<float>(new float[numberOfItems]);
            var dataSpan = new Span<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            var dataSpan2 = new Span<float>(dataMemory2.MemoryHandle.Pointer, numberOfItems);
            var dataSpan3 = new Span<float>(dataMemory3.MemoryHandle.Pointer, numberOfItems);
            var resultSpan = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);
            var dataDoubleSpan = new Span<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            var resultDoubleSpan = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < numberOfItems; i++)
            {                
                dataSpan[i] = i + 1.0f;
                data.Span[i] = i + 1.0f;
                data2.Span[i] = i + 1.0f;
                data3.Span[i] = i + 1.0f;
                dataSpan2[i] = i + 2.0f;
                dataSpan3[i] = i + 3.0f;
                resultSpan[i] = 0.0f;
                result.Span[i] = 0.0f;
                dataDoubleSpan[i] = i + 1.0;
                resultDoubleSpan[i] = 0.0;
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {           
            floatPool.Return(resultMemory, false);
            floatPool.Return(dataMemory, false);
            floatPool.Return(dataMemory2, false);
            floatPool.Return(dataMemory3, false);
            doublePool.Return(resultDoubleMemory, false);
            doublePool.Return(dataDoubleMemory, false);
            floatPool.Dispose();
            doublePool.Dispose();
        }

        
        [BenchmarkCategory("MultiplyAdd"), Benchmark(Baseline = true)]
        public unsafe void MultiplyAdd()
        {
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            var sp12 = new ReadOnlySpan<float>(dataMemory2.MemoryHandle.Pointer, numberOfItems);
            var sp13 = new ReadOnlySpan<float>(dataMemory3.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = sp1[i] * sp12[i] + sp13[i];
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddSpan()
        {            
            ReadOnlySpan<Vector256<float>> d1 = MemoryMarshal.Cast<float, Vector256<float>>(data.Span);
            ReadOnlySpan<Vector256<float>> d2 = MemoryMarshal.Cast<float, Vector256<float>>(data2.Span);
            ReadOnlySpan<Vector256<float>> d3 = MemoryMarshal.Cast<float, Vector256<float>>(data3.Span);
            Span<Vector256<float>> r = MemoryMarshal.Cast<float, Vector256<float>>(result.Span);

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddSpanAMH()
        {
            //int step = Vector256<float>.Count;            
            
            ReadOnlySpan<Vector256<float>> d1 = MemoryMarshal.Cast<byte, Vector256<float>>(new Span<byte>(dataMemory.MemoryHandle.Pointer, dataMemory.ByteArrayLength));
            ReadOnlySpan<Vector256<float>> d2 = MemoryMarshal.Cast<byte, Vector256<float>>(new Span<byte>(dataMemory2.MemoryHandle.Pointer, dataMemory2.ByteArrayLength));
            ReadOnlySpan<Vector256<float>> d3 = MemoryMarshal.Cast<byte, Vector256<float>>(new Span<byte>(dataMemory3.MemoryHandle.Pointer, dataMemory3.ByteArrayLength));
            Span<Vector256<float>> r = MemoryMarshal.Cast<byte, Vector256<float>>(new Span<byte>(resultMemory.MemoryHandle.Pointer, resultMemory.ByteArrayLength));

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddAMHPtr()
        {
            int step = Vector256<float>.Count;

            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr12 = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr13 = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Fma.MultiplyAdd(Avx.LoadAlignedVector256(currSpPtr), Avx.LoadAlignedVector256(currSpPtr12), Avx.LoadAlignedVector256(currSpPtr13)));
                currSpPtr += step;
                currSpPtr12 += step;
                currSpPtr13 += step;
                currSpPtr2 += step;
            }
        }

        [BenchmarkCategory("Negative MultiplyAdd"), Benchmark(Baseline = true)]
        public unsafe void NegMultiplyAdd()
        {
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            var sp12 = new ReadOnlySpan<float>(dataMemory2.MemoryHandle.Pointer, numberOfItems);
            var sp13 = new ReadOnlySpan<float>(dataMemory3.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = -(sp1[i] * sp12[i]) + sp13[i];
            }
        }

        [BenchmarkCategory("Negative MultiplyAdd"), Benchmark]
        public unsafe void FmaNegMultiplyAdd()
        {
            int step = Vector256<float>.Count;

            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr12 = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr13 = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Fma.MultiplyAddNegated(Avx.LoadAlignedVector256(currSpPtr), Avx.LoadAlignedVector256(currSpPtr12), Avx.LoadAlignedVector256(currSpPtr13)));
                currSpPtr += step;
                currSpPtr12 += step;
                currSpPtr13 += step;
                currSpPtr2 += step;
            }
        }
        
        
        [BenchmarkCategory("Reciprocal"), Benchmark(Baseline = true)]
        public unsafe void Reciprocal()
        {
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = 1.0f / sp1[i];
            }
        }       
        
        [BenchmarkCategory("Reciprocal"), Benchmark]
        public unsafe void ReciprocalDouble()
        {
            var sp1 = new ReadOnlySpan<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = 1.0 / sp1[i];
            }
        }


        [BenchmarkCategory("Reciprocal"), Benchmark]
        public unsafe void VectorReciprocal()
        {            
            int step = Vector256<float>.Count;

            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Avx.Reciprocal(Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }        

        
        [BenchmarkCategory("Reciprocal"), Benchmark]
        public unsafe void VecReciprocal()
        {                                    
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            ReadOnlySpan<Vector<float>> vecSpan = MemoryMarshal.Cast<float, Vector<float>>(sp1);

           var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);
            Span<Vector<float>> vecSpan2 = MemoryMarshal.Cast<float, Vector<float>>(sp2);           

            for (int i = 0; i < vecSpan.Length; i++)
            {
                vecSpan2[i] = Vector<float>.One / vecSpan[i];                
            }
        }
        
        [BenchmarkCategory("Reciprocal"), Benchmark]
        public unsafe void VectorReciprocalDouble()
        {            
            double one = 1.0;
            double* onePtr = &one;

            int step = Vector256<double>.Count;
            
            Vector256<double> oneVector = Avx.BroadcastScalarToVector256(onePtr);

            double* currSpPtr = (double*)dataMemory.MemoryHandle.Pointer;
            double* currSpPtr2 = (double*)resultDoubleMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Avx.Divide(oneVector, Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }
        /*
        [Benchmark]
        public unsafe void RecSquareRoot()
        {
            ReadOnlySpan<float> sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            Span<float> sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = 1.0f / MathF.Sqrt(sp1[i]);
            }
        }

        [Benchmark]
        public unsafe void RecSquareRootDouble()
        {
            ReadOnlySpan<double> sp1 = new ReadOnlySpan<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            Span<double> sp2 = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = 1.0 / Math.Sqrt(sp1[i]);
            }
        }

        [Benchmark]
        public unsafe void VectorRecSquareRoot()
        {
            int step = Vector256<float>.Count;
            
            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Avx.Reciprocal(Avx.Sqrt(Avx.LoadAlignedVector256(currSpPtr))));                
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }

        [Benchmark]
        public unsafe void VectorReciprocalSqrt()
        {
            int step = Vector256<float>.Count;

            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Avx.ReciprocalSqrt(Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }

        [Benchmark]
        public unsafe void VectorRecSquareRootDouble()
        {
            double one = 1.0;
            double* onePt = &one;
            Vector256<double> oneVec = Avx.BroadcastScalarToVector256(onePt);

            int step = Vector256<double>.Count;

            double* currSpPtr = (double*)dataMemory.MemoryHandle.Pointer;
            double* currSpPtr2 = (double*)resultDoubleMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {

                Avx.StoreAligned(currSpPtr2, Avx.Divide(oneVec, Avx.Sqrt(Avx.LoadAlignedVector256(currSpPtr))));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        } */

        [BenchmarkCategory("Square root"), Benchmark(Baseline = true)]
        public unsafe void SquareRoot()
        {
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = MathF.Sqrt(sp1[i]);
            }
        }

        [BenchmarkCategory("Square root"), Benchmark]
        public unsafe void SquareRootDouble()
        {
            var sp1 = new ReadOnlySpan<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfItems);
            var sp2 = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = Math.Sqrt(sp1[i]);
            }
        }

        [BenchmarkCategory("Square root"), Benchmark]
        public unsafe void VectorSquareRoot()
        {            
            int step = Vector256<float>.Count;
            float* currSpPtr = (float*)dataMemory.MemoryHandle.Pointer;
            float* currSpPtr2 = (float*)resultMemory.MemoryHandle.Pointer;

            for (int i = 0; i < numberOfItems; i += step)
            {
                Avx.StoreAligned(currSpPtr2, Avx.Sqrt(Avx.LoadAlignedVector256(currSpPtr)));
                currSpPtr += step;
                currSpPtr2 += step;
            }
        }

        [BenchmarkCategory("Square root"), Benchmark]
        public unsafe void VecSquareRoot()
        {            
            var sp1 = new ReadOnlySpan<float>(dataMemory.MemoryHandle.Pointer, numberOfItems);
            ReadOnlySpan<Vector<float>> vecSpan = MemoryMarshal.Cast<float, Vector<float>>(sp1);

            var sp2 = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfItems);
            Span<Vector<float>> vecSpan2 = MemoryMarshal.Cast<float, Vector<float>>(sp2);

            for (int i = 0; i < vecSpan.Length; i++)
            {
                vecSpan2[i] = Vector.SquareRoot<float>(vecSpan[i]);
            }
        }
    }

}

