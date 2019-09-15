using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.InteropServices;

namespace Core3Intrinsics
{
    public static class Complex
    {
        private static readonly int vecFloatCount= Vector256<float>.Count; 
        public struct FloatComplex
        {
            public float X, Y;

            public FloatComplex((float real, float imaginary) floatTuple)
            {
                X = floatTuple.real;
                Y = floatTuple.imaginary;
            }

            public FloatComplex(float real, float imaginary)
            {
                X = real;
                Y = imaginary;
            }
        }

        public struct VectorFloatComplex
        {
            public Vector256<float> X, Y;

            public VectorFloatComplex(Vector256<float> real, Vector256<float> imaginary)
            {
                X = real;
                Y = imaginary;
            }

            public unsafe VectorFloatComplex(ref float[] left, ref float[] right, int startIndex = 0)
            {
                fixed (float* ptLeft = &left[startIndex])
                {
                    X = Avx.LoadVector256(ptLeft);
                }
                fixed (float* ptRight = &right[startIndex])
                {
                    Y = Avx.LoadVector256(ptRight);
                }
                
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatComplex Add(FloatComplex left, FloatComplex right)
        {
            return new FloatComplex(left.X + right.X, left.Y + right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatComplex Multiply(FloatComplex left, FloatComplex right)
        {
            return new FloatComplex((left.X * right.X) - (left.Y * right.Y), (left.X * right.Y) + (left.Y * right.X));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Modulus(FloatComplex left)
        {
            return MathF.Sqrt(left.X * left.X + left.Y * left.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ModulusSquared(FloatComplex left)
        {
            return left.X * left.X + left.Y * left.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorFloatComplex Add(VectorFloatComplex left, VectorFloatComplex right)
        {
            return new VectorFloatComplex(Avx.Add(left.X, right.X), Avx.Add(left.Y, right.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorFloatComplex Multiply(VectorFloatComplex left, VectorFloatComplex right)
        {
            Vector256<float> real = Avx.Subtract(Avx.Multiply(left.X, right.X), Avx.Multiply(left.Y, right.Y));
            Vector256<float> imaginary = Avx.Add(Avx.Multiply(left.X, right.Y), Avx.Multiply(left.Y, right.X));
            return new VectorFloatComplex(real, imaginary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Modulus(VectorFloatComplex left)
        {            
            return Avx.Sqrt(Avx.Add(Avx.Multiply(left.X, left.X), Avx.Multiply(left.Y, left.Y)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> ModulusSquared(VectorFloatComplex left)
        {
            return Avx.Add(Avx.Multiply(left.X, left.X), Avx.Multiply(left.Y, left.Y));
        }
    }
}
