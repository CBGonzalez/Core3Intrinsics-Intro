using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.InteropServices;

namespace Core3IntrinsicsBenchmarks
{
    public struct ComplexF
    {
        private static readonly int vecFloatCount = Vector256<float>.Count;
        private float m_real, m_imaginary;

        public float X => m_real;
        public float Y => m_imaginary;

        public ComplexF((float real, float imaginary) floatTuple)
        {
            m_real = floatTuple.real;
            m_imaginary = floatTuple.imaginary;
        }

        public ComplexF(float real, float imaginary)
        {
            m_real = real;
            m_imaginary = imaginary;
        }
        

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComplexF Add(ComplexF left, ComplexF right)
        {
            return new ComplexF(left.X + right.X, left.Y + right.Y);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComplexF Multiply(ComplexF left, ComplexF right)
        {
            return new ComplexF((left.X * right.X) - (left.Y * right.Y), (left.X * right.Y) + (left.Y * right.X));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Modulus(ComplexF left)
        {
            return MathF.Sqrt(left.X * left.X + left.Y * left.Y);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ModulusSquared(ComplexF left)
        {
            return left.X * left.X + left.Y * left.Y;
        }

        public static float Abs(ComplexF value)
        {

            if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
            {
                return float.PositiveInfinity;
            }

            // |value| == sqrt(a^2 + b^2)
            // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
            // Using the above we can factor out the square of the larger component to dodge overflow.


            float c = MathF.Abs(value.m_real);
            float d = MathF.Abs(value.m_imaginary);

            if (c > d)
            {
                float r = d / c;
                return c * MathF.Sqrt(1.0f + r * r);
            }
            else if (d == 0.0f)
            {
                return c;  // c is either 0.0 or NaN
            }
            else
            {
                float r = c / d;
                return d * MathF.Sqrt(1.0f + r * r);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSquared(ComplexF value)
        {

            //if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
            //{
            //    return float.PositiveInfinity;
            //}

            // |value| == sqrt(a^2 + b^2)
            // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
            // Using the above we can factor out the square of the larger component to dodge overflow.


            //float c = MathF.Abs(value.m_real);
            //float d = MathF.Abs(value.m_imaginary);

            return value.m_real * value.m_real + value.m_imaginary * value.m_imaginary;// c * c + d * d;
            //if (c > d)
            //{
            //    float r = d / c;
            //    return c * MathF.Sqrt(1.0f + r * r);
            //}
            //else if (d == 0.0f)
            //{
            //    return c;  // c is either 0.0 or NaN
            //}
            //else
            //{
            //    float r = c / d;
            //    return d * MathF.Sqrt(1.0f + r * r);
            //}
        }

        public static ComplexF Squared(ComplexF left)
        {
            return new ComplexF(MathF.FusedMultiplyAdd(left.m_real, left.m_real,  -(left.m_imaginary * left.m_imaginary)), 2.0f * left.m_real * left.m_imaginary);
        }

        public static ComplexF FusedMultiplyAdd(ComplexF left, ComplexF right, ComplexF summand)
        {
            return new ComplexF(MathF.FusedMultiplyAdd(left.X, right.X, summand.X), MathF.FusedMultiplyAdd(left.Y, right.Y, summand.Y));
        }

        public static ComplexF operator +(ComplexF left, ComplexF right)
        {
            return (new ComplexF((left.m_real + right.m_real), (left.m_imaginary + right.m_imaginary)));

        }

        public static ComplexF operator -(ComplexF left, ComplexF right)
        {
            return (new ComplexF((left.m_real - right.m_real), (left.m_imaginary - right.m_imaginary)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComplexF operator *(ComplexF left, ComplexF right)
        {
            // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i           
            //float result_Realpart = (left.m_real * right.m_real) - (left.m_imaginary * right.m_imaginary);
            float result_Realpart = MathF.FusedMultiplyAdd(left.m_real, right.m_real, -(left.m_imaginary * right.m_imaginary));
            //float result_Imaginarypart = (left.m_imaginary * right.m_real) + (left.m_real * right.m_imaginary);
            float result_Imaginarypart = MathF.FusedMultiplyAdd(left.m_imaginary, right.m_real, (left.m_real * right.m_imaginary));
            return (new ComplexF(result_Realpart, result_Imaginarypart));
        }
    }
}
