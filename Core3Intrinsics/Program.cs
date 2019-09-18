using System;
using System.Runtime.InteropServices;

namespace Core3Intrinsics
{
    class Program
    {
        static Loading ld;
        static unsafe void Main()
        {
            /*Console.WriteLine("Testing");
            float real1 = 2.5f;
            float ima1 = -3.5f;
            float real2 = 3.5f;
            float ima2 = 100.5f;
            Complex.FloatComplex comp1 = new Complex.FloatComplex(real1, ima1);
            Complex.FloatComplex comp2 = new Complex.FloatComplex(real2, ima2);
            System.Diagnostics.Debug.Assert(real1 + real2 == Complex.Add(comp1, comp2).X);
            System.Diagnostics.Debug.Assert(ima1 + ima2 == Complex.Add(comp1, comp2).Y);
            float mo = real1 * real1 + ima1 * ima1;
            System.Diagnostics.Debug.Assert(mo == Complex.ModulusSquared(comp1));
            mo = real1 * real2 - ima1 * ima2;// + real1 * ima2 + ima1 * real2;
            System.Diagnostics.Debug.Assert(mo == Complex.Multiply(comp1, comp2).X);
            mo = real1 * ima2 + ima1 * real2;
            System.Diagnostics.Debug.Assert(mo == Complex.Multiply(comp1, comp2).Y);
            _ = Console.ReadLine(); */
            Intro intro = new Intro();
            ld = new Loading();
            Console.WriteLine("Starting ...");
            Console.WriteLine("\tMandelBrot");
            Mandelbrot man = new Mandelbrot();
            man.FloatMandel();
            man.FloatMandelComplex();
            (bool areEqual, System.Collections.Generic.List<int> errorList, int maxDifference) = Validator.CompareValues(man.results.Span.ToArray(), man.results2.Span.ToArray());
            Console.WriteLine($"\t\tMandelBrot successful: {areEqual}, Number of differences: {errorList.Count}, max. difference: {maxDifference}");
            Console.WriteLine($"\t\tDone with mandelbrot, total bytes: {man.SizeInBytes}");
            _ = Console.ReadLine();
            return;
            //
            //_ = Console.ReadLine();
            //ReadOnlySpan<float> sp1 = man.results.Span;
            //ReadOnlySpan<float> sp2 = man.results2.Span;
            //ld.SquareRoot();
            //ld.VectorSquareRoot();
            //(bool areEqual, System.Collections.Generic.List<int> errorList) = Validator.CompareValues(ld.storeMem.Memory.Span.ToArray(), ld.store2Mem.Memory.Span.ToArray());
            //Console.WriteLine($"\t\tMandelBrot successful: {areEqual}, Number of differences: {errorList.Count}");
            //Console.WriteLine($"\t\tDone with mandelbrot, total bytes: {ld.dataMem.Memory.Span.Length * sizeof(float)}");
            //_ = Console.ReadLine();
            //
            //Console.ReadLine();
            //bool areEqual2 = true;
            //for (int i = 0; i < sp1.Length; i++)
            //{
            //    areEqual2 &= sp1[i] == sp2[i];
            //    if (!areEqual2)
            //    {
            //        Console.WriteLine($"Difference at index {i}");
            //        break;
            //    }
            //}
            Console.WriteLine($"Done.");
            Console.ReadLine();
            return;
            /*ld = new Loading();
            //ld.ScalarStore();
            ld.SquareRoot();
            Console.WriteLine("Done scalar");
            //ld.VectorStore();
            ld.VectorSquareRoot();
            bool areEqual = true;
            //ReadOnlySpan<float> sp1 = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(Loading.genericAlignedStoreFloat.BufferIntPtr.ToPointer(), Loading.genericAlignedStoreFloat.ByteLength));
            ReadOnlySpan<float> sp1 = new ReadOnlySpan<float>(Loading.resultMemHandle.Pointer, Loading.alignedStoreBuffer2.ByteLength);
            ReadOnlySpan<float> sp2 = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(Loading.alignedStoreBuffer2.GetPointer().ToPointer(), Loading.alignedStoreBuffer2.ByteLength));
            for (int i = 0; i < Loading.alignedStoreBuffer.TLength; i++)
            {
                areEqual &= sp1[i] == sp2[i];
                if (!areEqual)
                {
                    Console.WriteLine($"Differenc at index {i}");
                    break;
                }
            }
            Console.WriteLine("Done vector");
            //ld.VectorStoreUnaligned();
            ld.RVectorSquareRoot();
            Console.WriteLine("Done scalar rsqrt");
            ld.RVectorSquareRootDouble();
            Console.WriteLine("Done vector rsqrt");
           
            ld.Dispose();
            //Console.WriteLine("Done vector unaligned");
            Console.WriteLine($"Done, results are equal = {areEqual}");
            Console.ReadLine();*/
        }
    }
}
