
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3Intrinsics
{
    public class Intro
    {
        public Intro()
        {
            //Vector128<float> middleVector = Vector128.Create(1.0f);  // middleVector = <1,1,1,1>
            //middleVector = Vector128.CreateScalar(-1.0f);  // middleVector = <-1,0,0,0>
            //Vector64<byte> floatBytes = Vector64.AsByte(Vector64.Create(1.0f, -1.0f)); // floatBytes = <0, 0, 128, 63, 0, 0, 128, 191>
            if(Avx.IsSupported)
            {
                var left = Vector256.Create(-2.5f);
                var right = Vector256.Create(5.0f);
                Vector256<float> result = Avx.Add(left, right); // result = <2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5>
                result = Avx.Multiply(left, right); // result = <-12.5, -12.5, -12.5, -12.5, -12.5, -12.5, -12.5, -12.5>

                double[] someDoubles = new double[] { 1.0, 3.0, -2.5, 7.5, 10.8, 0.33333 };
                unsafe
                {
                    fixed (double* ptr = &someDoubles[1])
                    {
                        Vector256<double> res2 = Avx.LoadVector256(ptr); // res2 = <3, -2.5, 7.5, 10.8>
                    }
                }
            }
        }
    }
}
