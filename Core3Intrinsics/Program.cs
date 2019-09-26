using System;
using System.Runtime.InteropServices;

namespace Core3Intrinsics
{
    class Program
    {
        static unsafe void Main()
        {
            Console.WriteLine("Starting test ...");
            Console.WriteLine("\tMandelBrot");
            var man = new Mandelbrot();
            man.FloatMandel();
            man.Vector256Mandel();
            (bool areEqual, System.Collections.Generic.List<int> errorList, int maxDifference) = Validator.CompareValuesFloat(man.results.Span.ToArray(), man.results2.Span.ToArray());
            Console.WriteLine($"\t\tMandelBrot successful: {areEqual}, Number of differences: {errorList.Count}, max. difference: {maxDifference}");
            Console.WriteLine($"\t\tDone with mandelbrot, total bytes: {man.SizeInBytes}");
            //Transpose.CreateArrays();
            //bool res1 = Transpose.SerializeColorsInt();
            //bool res2 = Transpose.SerializedColorsVector256();
            //if(res1 && res2)
            //{
            //    (bool areEqual, System.Collections.Generic.List<int> errorList) = Validator.CompareValues<int>(Transpose.transposed1, Transpose.transposed2);
            //    Console.WriteLine($"Transpose ended with success {areEqual}, number of differences {errorList.Count}");
            //}
            //else
            //{
            //    Console.WriteLine($"Error running Transpose");
            //}
            _ = Console.ReadLine();
        }
    }
}
