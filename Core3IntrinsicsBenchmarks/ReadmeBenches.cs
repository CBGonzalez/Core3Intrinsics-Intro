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
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
    public class ReadmeBenches
    {
        [Params(4096/*, 1048576*/)]
        public int NumberOfFloats { get; set; }

        private static float[] inputData;

        [GlobalSetup]
        public void GlobalSetup()
        {
            inputData = new float[NumberOfFloats];
            for(int i = 0; i < inputData.Length; i++)
            {
                inputData[i] = i + 1;
            }
        }

        [Benchmark(Baseline = true)]
        public float[] ProcessData()
        {
            var left = Vector256.Create(-2.5f); // <-2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5>
            var right = Vector256.Create(5.0f); // <5, 5, 5, 5, 5, 5, 5, 5>            
            Vector256<float> result = Avx.DotProduct(left, right, 0b1111_0001); // result = <-50, 0, 0, 0, -50, 0, 0, 0>
            float[] results = new float[inputData.Length];
            Span<Vector256<float>> resultVectors = MemoryMarshal.Cast<float, Vector256<float>>(results);

            ReadOnlySpan<Vector256<float>> inputVectors = MemoryMarshal.Cast<float, Vector256<float>>(inputData);

            for (int i = 0; i < inputVectors.Length; i++)
            {
                resultVectors[i] = Avx.Sqrt(inputVectors[i]);
            }
            results[0] = result.GetElement(0);
            return results;
        }

        [Benchmark]
        public unsafe float[] ProcessDataUnsafe()
        {
            float[] results = new float[inputData.Length];
            fixed (float* inputPtr = &inputData[0])
            {
                float* inCurrent = inputPtr;
                fixed (float* resultPtr = &results[0])
                {
                    float* resEnd = resultPtr + results.Length;
                    float* resCurrent = resultPtr;
                    while (resCurrent < resEnd)
                    {
                        Avx.Store(resCurrent, Avx.Sqrt(Avx.LoadVector256(inCurrent)));
                        resCurrent += 8;
                        inCurrent += 8;
                    }
                }
            }
            return results;
        }

    }
}
