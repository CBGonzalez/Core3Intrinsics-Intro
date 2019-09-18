using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3IntrinsicsBenchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
    //[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    //[CategoriesColumn]
    public class BasicOps
    {
        [Params(256 * 1024, 10 * 4 * 1024 * 1024)]
        public int ParamCacheSizeBytes { get; set; }

        private int numberOfFloatItems, numberOfDoubleItems;
        private static int algn = 32;       
        private AlignedArrayPool<float> floatPool;
        private AlignedArrayPool<double> doublePool;
        private AlignedMemoryHandle<float> dataMemory, dataMemory2, dataMemory3, resultMemory;//
        private AlignedMemoryHandle<double> dataDoubleMemory, resultDoubleMemory;
        private float[] data, data2, data3, result;
        private double[] dataD, dataD2, dataD3, resultD;

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            numberOfFloatItems = ParamCacheSizeBytes / sizeof(float) / 4; // make sure that all data fits
            numberOfDoubleItems = ParamCacheSizeBytes / sizeof(double) / 4;
            floatPool = new AlignedArrayPool<float>();
            doublePool = new AlignedArrayPool<double>();
            dataMemory = floatPool.Rent(numberOfFloatItems);
            dataMemory2 = floatPool.Rent(numberOfFloatItems);
            dataMemory3 = floatPool.Rent(numberOfFloatItems);
            resultMemory = floatPool.Rent(numberOfFloatItems);
            dataDoubleMemory = doublePool.Rent(numberOfDoubleItems);
            resultDoubleMemory = doublePool.Rent(numberOfDoubleItems);
            data = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            data2 = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            data3 = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            result = ArrayPool<float>.Shared.Rent(numberOfFloatItems);
            dataD =   ArrayPool<double>.Shared.Rent(numberOfDoubleItems);
            dataD2 =  ArrayPool<double>.Shared.Rent(numberOfDoubleItems);
            dataD3 =  ArrayPool<double>.Shared.Rent(numberOfDoubleItems);
            resultD = ArrayPool<double>.Shared.Rent(numberOfDoubleItems);
            var dataSpan = new Span<float>(dataMemory.MemoryHandle.Pointer, numberOfFloatItems);
            var dataSpan2 = new Span<float>(dataMemory2.MemoryHandle.Pointer, numberOfFloatItems);
            var dataSpan3 = new Span<float>(dataMemory3.MemoryHandle.Pointer, numberOfFloatItems);
            var resultSpan = new Span<float>(resultMemory.MemoryHandle.Pointer, numberOfFloatItems);
            var dataDoubleSpan = new Span<double>(dataDoubleMemory.MemoryHandle.Pointer, numberOfDoubleItems);
            var resultDoubleSpan = new Span<double>(resultDoubleMemory.MemoryHandle.Pointer, numberOfDoubleItems);

            for (int i = 0; i < numberOfFloatItems; i++)
            {                
                dataSpan[i] = i + 1.0f;
                data[i] = i + 1.0f;
                data2[i] = i + 1.0f;
                data3[i] = i + 1.0f;
                dataSpan2[i] = i + 2.0f;
                dataSpan3[i] = i + 3.0f;
                resultSpan[i] = 0.0f;
                result[i] = 0.0f;                
            }
            for(int i = 0; i < numberOfDoubleItems; i++)
            {
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
            ArrayPool<float>.Shared.Return(data);
            ArrayPool<float>.Shared.Return(data2);
            ArrayPool<float>.Shared.Return(data3);
            ArrayPool<float>.Shared.Return(result);
            ArrayPool<double>.Shared.Return(dataD);
            ArrayPool<double>.Shared.Return(dataD2);
            ArrayPool<double>.Shared.Return(dataD3);
            ArrayPool<double>.Shared.Return(resultD);
        }

        
        /*[BenchmarkCategory("MultiplyAdd"), Benchmark(Baseline = true)]
        public unsafe void MultiplyAddScalarFloat()
        {
            var sp1 = new ReadOnlySpan<float>(data, 0, numberOfFloatItems);
            var sp12 = new ReadOnlySpan<float>(data2, 0, numberOfFloatItems);
            var sp13 = new ReadOnlySpan<float>(data3, 0, numberOfFloatItems);
            var sp2 = new Span<float>(result, 0, numberOfFloatItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = sp1[i] * sp12[i] + sp13[i];
            }
        } */

        [BenchmarkCategory("MultiplyAdd"), Benchmark(Baseline = true)]
        public unsafe void ScalarFloatMultipleOps()
        {
            var sp1 = new ReadOnlySpan<float>(data, 0, numberOfFloatItems);
            var sp12 = new ReadOnlySpan<float>(data2, 0, numberOfFloatItems);
            var sp13 = new ReadOnlySpan<float>(data3, 0, numberOfFloatItems);
            var sp2 = new Span<float>(result, 0, numberOfFloatItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = sp1[i] * sp12[i] + sp13[i];
                sp2[i] = sp2[i] * sp1[i] + sp1[i];
                sp2[i] = sp1[i] * sp1[i] + sp2[i];
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void Vector256FloatMultipleOps()
        {
            ReadOnlySpan<Vector256<float>> d1 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d2 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data2, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d3 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data3, 0, numberOfFloatItems));
            Span<Vector256<float>> r = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(result, 0, numberOfFloatItems));

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
                r[i] = Fma.MultiplyAdd(r[i], d1[i], d1[i]);
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], r[i]);
            }
        }
        /*
        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void MultiplyAddScalarDouble()
        {
            var sp1 = new ReadOnlySpan<double>(dataD, 0, numberOfDoubleItems);
            var sp12 = new ReadOnlySpan<double>(dataD2, 0, numberOfDoubleItems);
            var sp13 = new ReadOnlySpan<double>(dataD3, 0, numberOfDoubleItems);
            var sp2 = new Span<double>(resultD, 0, numberOfDoubleItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = sp1[i] * sp12[i] + sp13[i];
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddvector256Float()
        {            
            ReadOnlySpan<Vector256<float>> d1 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d2 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data2, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d3 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data3, 0, numberOfFloatItems));
            Span<Vector256<float>> r = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(result, 0, numberOfFloatItems));

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
            }
        }

        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddvector256Double()
        {
            ReadOnlySpan<Vector256<double>> d1 = MemoryMarshal.Cast<double, Vector256<double>>(new Span<double>(dataD, 0, numberOfDoubleItems));
            ReadOnlySpan<Vector256<double>> d2 = MemoryMarshal.Cast<double, Vector256<double>>(new Span<double>(dataD2, 0, numberOfDoubleItems));
            ReadOnlySpan<Vector256<double>> d3 = MemoryMarshal.Cast<double, Vector256<double>>(new Span<double>(dataD3, 0, numberOfDoubleItems));
            Span<Vector256<double>> r = MemoryMarshal.Cast<double, Vector256<double>>(new Span<double>(resultD, 0, numberOfDoubleItems));

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
            }
        } /*

        /*
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
        } */
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
        /*
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
        } */
    }

}

