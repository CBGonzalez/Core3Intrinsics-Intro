using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using System.Collections.Generic;

namespace Core3IntrinsicsBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {

            //var summary = BenchmarkRunner.Run<MemoryBenches>(new DebugInProcessConfig());
            BenchmarkDotNet.Reports.Summary summary = BenchmarkRunner.Run<MemoryBenches>();
            
            //Console.WriteLine(summary.Title);
            
            int meanIndex = -1, numberOfFloatsIndex = -1, methodIndex = -1, index = 0;
            foreach(string header in summary.Table.FullHeader)
            {
                if(header == "Mean")
                {
                    meanIndex = index;
                }
                if(header == "numberOfBytes")
                {
                    numberOfFloatsIndex = index;
                }
                if (header == "Method")
                {
                   methodIndex = index;
                }
                index++;
                if(meanIndex >= 0 && numberOfFloatsIndex >= 0 && methodIndex >= 0)
                {
                    break;
                }
            }
            if (meanIndex >= 0 && numberOfFloatsIndex >= 0 && methodIndex >= 0)
            {
                Console.WriteLine("\nCustom results **************************");
                string meanValue, numberOfItemsValueString, methodString;
                int numberOfBytes = -1;
                double throughPut = -1.0;
                List<string> bytesMoved = new List<string>();
                List<string> methods = new List<string>();
                List<string> throughPuts = new List<string>();
                int maxCharsMoved = 0, maxCharsMethod = 0, maxCharsThrou = 0; ;
                string[][] fullContent = summary.Table.FullContent;
                for (int i = 0; i < fullContent.Length; i++)
                {
                    meanValue = fullContent[i][meanIndex];
                    methodString = fullContent[i][methodIndex];
                    methods.Add(methodString);
                    if(maxCharsMethod < methodString.Length + 2)
                    {
                        maxCharsMethod = methodString.Length + 2;
                    }
                    (double thisTimeFactor, double value) = (GetTimeFactor(meanValue));
                    numberOfItemsValueString = fullContent[i][numberOfFloatsIndex];
                    numberOfBytes = int.Parse(numberOfItemsValueString);
                    throughPut = numberOfBytes / value / thisTimeFactor;
                    string thrUnit = "KB/s";
                    if(throughPut > 1024 * 1024 * 1024)
                    {
                        throughPut /= 1024.0 * 1024.0 * 1024.0;
                        thrUnit = "GB/s";
                    }
                    else
                    {
                        if(throughPut > 1024 * 1024)
                        {
                            throughPut /= 1024.0 * 1024.0;
                            thrUnit = "MB/s";
                        }
                    }
                    bytesMoved.Add(numberOfBytes.ToString("N0"));
                    if(numberOfBytes.ToString("N0").Length + 2 > maxCharsMoved)
                    {
                        maxCharsMoved = numberOfBytes.ToString("N0").Length + 2;
                    }
                    string throughString = $"{throughPut.ToString("N2")} {thrUnit}";
                    throughPuts.Add(throughString);
                    if (throughString.Length +2 > maxCharsThrou)
                    {
                        maxCharsThrou = throughString.Length + 2;
                    }                    
                }                
                int len = "| Bytes moved".Length;
                int pos1 = len >= maxCharsMoved ? len + 2 : maxCharsMoved + 2;
                int pos2 = pos1 + maxCharsMethod + 2;
                string allSpaces = "                                                                                                                          ";
                string aux = allSpaces.Insert(0, "| Bytes moved").Insert(pos1, "| Method").Insert(pos2, "| Throughput").TrimEnd();
               
                Console.WriteLine(aux);
                string currBytesMoved = bytesMoved[0];
                for(int i = 0; i < methods.Count; i++)
                {
                    if(currBytesMoved != bytesMoved[i])
                    {
                        Console.WriteLine();
                        currBytesMoved = bytesMoved[i];
                    }
                    Console.WriteLine($"{allSpaces.Insert(0, "| " + bytesMoved[i]).Insert(pos1, "| " + methods[i]).Insert(pos2, "| " + throughPuts[i]).TrimEnd()}");
                }
            }
            else
            {
                Console.WriteLine("No custom results.");
            }

            return;

            (double, double) GetTimeFactor(string unitsString)
            {
                double res1 = -1.0, res2 = -1.0;
                string unit = unitsString.Substring(unitsString.IndexOf(' ')).Trim();
                string value = unitsString.Substring(0, unitsString.IndexOf(' '));
                double.TryParse(value, out res2);
                switch(unit)
                {
                    case "ns":
                        res1 = 1 / 1_000_000_000.0;
                        break;
                    case "us":
                        res1 = 1 / 1_000_000.0;
                        break;
                    case "ms":
                        res1 = 1 / 1_000.0;
                        break;
                    case "s":
                        res1 = 1.0;
                        break;
                }
                return (res1, res2);
            }
            //var summary = BenchmarkRunner.Run<BasicOps>();
            //BenchmarkDotNet.Reports.Summary summary = BenchmarkRunner.Run<IntegerBasicOps>();
            //var summary = BenchmarkRunner.Run<TrigonometricOps>();
            //var summary = BenchmarkRunner.Run<Mandelbrot>();
        }
    }
}
