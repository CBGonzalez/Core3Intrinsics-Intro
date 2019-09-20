using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using System.Collections.Generic;

namespace Core3IntrinsicsBenchmarks
{
    class Program
    {
        static void Main()
        {
            //var summary = BenchmarkRunner.Run<MemoryBenches>();
            //_ = BenchmarkRunner.Run<BasicOps>();
            var summary = BenchmarkRunner.Run<IntegerBasicOps>();
            //var summary = BenchmarkRunner.Run<TrigonometricOps>();
            //var summary = BenchmarkRunner.Run<Mandelbrot>();
            //var summary = BenchmarkRunner.Run<ReadmeBenches>();
        }
    }
}
