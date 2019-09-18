
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3Intrinsics
{
    public class Intro
    {
        public Intro()
        {
            Vector128<float> middleVector = Vector128.Create(1.0f);  // middleVector = <1,1,1,1>
            middleVector = Vector128.CreateScalar(-1.0f);  // middleVector = <-1,0,0,0>
            Vector64<byte> floatBytes = Vector64.AsByte(Vector64.Create(1.0f, -1.0f)); // floatBytes = <0, 0, 128, 63, 0, 0, 128, 191>
            if(Avx.IsSupported)
            {
                var left = Vector256.Create(-2.5f); // <-2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5>
                var right = Vector256.Create(5.0f); // <5, 5, 5, 5, 5, 5, 5, 5>
                Vector256<float> result = Avx.AddSubtract(left, right); // result = <-7.5, 2.5, -7.5, 2.5, -7.5, 2.5, -7.5, 2.5>xit
                left = Vector256.Create(-1.0f, -2.0f, -3.0f, -4.0f, -50.0f, -60.0f, - 70.0f, -80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 4.0f, 50.0f, 60.0f, 70.0f, 80.0f);
                result = Avx.UnpackHigh(left, right); // result = <-3, 3, -4, 4, -70, 70, -80, 80>
                result = Avx.UnpackLow(left, right); // result = <-1, 1, -2, 2, -50, 50, -60, 60>
                result = Avx.DotProduct(left, right, 0b1111_0001); // result = <-30, 0, 0, 0, -17400, 0, 0, 0>
                bool testResult = Avx.TestC(left, right); // testResult = true
                testResult = Avx.TestC(right, left); // testResult = false
                var result1 = Avx.Divide(left, right);
                Vector256<float> plusOne = Vector256.Create(1.0f);
                result = Avx.Compare(right, result1, FloatComparisonMode.OrderedGreaterThanNonSignaling);
                result = Avx.Compare(right, result1, FloatComparisonMode.UnorderedNotLessThanNonSignaling);
                left = Vector256.Create(0.0f, 3.0f, -3.0f, 4.0f, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var nanInFirstPosition = Avx.Divide(left, right);
                left = Vector256.Create(1.1f, 3.3333333f, -3.0f, 4.22f, -50.0f, 60.0f, -70.0f, 80.0f);
                var InfInFirstPosition = Avx.Divide(left, right);

                left = Vector256.Create(-1.1f, 3.0f, 1.0f/3.0f, MathF.PI, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.1f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var compareResult = Avx.Compare(left, right, FloatComparisonMode.OrderedGreaterThanNonSignaling); // compareResult = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
                Vector256<float> mixed = Avx.BlendVariable(left, right, compareResult); //  mixed = <-1, 2, -3, 2, -50, -60, -70, -80>

                //left = Vector256.Create(-1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f);
                //right = Vector256.Create(1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 1.0f);
                var other = right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                bool bRes = Avx.TestZ(plusOne, compareResult);
                bool bRes2 = Avx.TestC(plusOne, compareResult);
                bool allTrue = !Avx.TestZ(compareResult, compareResult);
                compareResult = Avx.Compare(nanInFirstPosition, right, FloatComparisonMode.OrderedEqualNonSignaling); // compareResult = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
                compareResult = Avx.Compare(nanInFirstPosition, right, FloatComparisonMode.UnorderedEqualNonSignaling);
                compareResult = Avx.Compare(InfInFirstPosition, right, FloatComparisonMode.UnorderedNotLessThanOrEqualNonSignaling);
                compareResult = Avx.Compare(InfInFirstPosition, right, FloatComparisonMode.OrderedGreaterThanNonSignaling);
                var left128 = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
                var right128 = Vector128.Create(2.0f, 3.0f, 4.0f, 5.0f);
                Vector128<float> compResult128 = Sse.CompareGreaterThan(left128, right128); // compResult128 = <0, 0, 0, 0>
                
                int res = Avx.MoveMask(compareResult);
                if (Fma.IsSupported)
                {
                    var resultFma = Fma.MultiplyAdd(left, right, other); // = left * right + other for each element
                    resultFma = Fma.MultiplyAddNegated(left, right, other); // = -(left * right + other) for each element
                    resultFma = Fma.MultiplySubtract(left, right, other); // = left * right - other for each element
                    Fma.MultiplyAddSubtract(left, right, other); // even elements (0, 2, ...) like MultiplyAdd, odd elements like MultiplySubtract 

                }
                result = Avx.DotProduct(left, right, 0b1010_0001); // result = <-20, 0, 0, 0, -10000, 0, 0, 0>
                result = Avx.Floor(left);  // result = <-3, -3, -3, -3, -3, -3, -3, -3>
                result = Avx.Add(left, right); // result = <2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5>
                result = Avx.Ceiling(left); // result = <-2, -2, -2, -2, -2, -2, -2, -2>
                result = Avx.Multiply(left, right); // result = <-12.5, -12.5, -12.5, -12.5, -12.5, -12.5, -12.5, -12.5>
                result = Avx.HorizontalAdd(left, right); // result = <-5, -5, 10, 10, -5, -5, 10, 10>
                result = Avx.HorizontalSubtract(left, right); // result = <0, 0, 0, 0, 0, 0, 0, 0>
                double[] someDoubles = new double[] { 1.0, 3.0, -2.5, 7.5, 10.8, 0.33333 };
                double[] someOtherDoubles = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
                double[] someResult = new double[someDoubles.Length];
                float[] someFloats = new float[] { 1, 2, 3, 4, 10, 20, 30, 40, 0 };
                float[] someOtherFloats = new float[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                unsafe
                {
                    fixed (double* ptr = &someDoubles[1])
                    {
                        fixed (double* ptr2 = &someResult[0])
                        {
                            Vector256<double> res2 = Avx.LoadVector256(ptr); // res2 = <3, -2.5, 7.5, 10.8>                            
                            Avx.Store(ptr2, res2);
                        }
                    }

                    fixed (float* ptr = &someFloats[0])
                    {
                        fixed (float* ptr2 = &someOtherFloats[0])
                        {
                            Vector256<float> res2 = Avx.DotProduct(Avx.LoadVector256(ptr), Avx.LoadVector256(ptr2), 0b0001_0001); 
                            //Avx.Store(ptr2, res2);
                        }
                    }
                }


                
            }
        }

        public float[] ProcessData(ref Span<float> input)
        {
            float[] results = new float[input.Length];
            Span<Vector256<float>> resultVectors = MemoryMarshal.Cast<float, Vector256<float>>(results);

            ReadOnlySpan<Vector256<float>> inputVectors = MemoryMarshal.Cast<float, Vector256<float>>(input);

            for(int i = 0; i < inputVectors.Length; i++)
            {
                resultVectors[i] = Avx.Sqrt(inputVectors[i]);                
            }

            return results;
        }

        public unsafe float[] ProcessDataUnsafe(ref Span<float> input)
        {
            float[] results = new float[input.Length];
            fixed (float* inputPtr = &input[0])
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
