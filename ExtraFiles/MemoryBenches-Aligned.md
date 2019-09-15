``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview9-014004
  [Host]     : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT


```
|                       Method | NumberOfBytes |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD |
|----------------------------- |-------------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|
|           **VectorStoreAligned** |         **16384** |       **504.7 ns** |      **7.635 ns** |      **8.792 ns** |       **504.6 ns** |  **1.00** |    **0.00** |
|       VectorStoreArrayMemPtr |         16384 |       385.1 ns |      6.161 ns |      4.810 ns |       383.8 ns |  0.76 |    0.01 |
|      VectorStoreArrayMemSafe |         16384 |       597.0 ns |     11.873 ns |     12.193 ns |       595.5 ns |  1.18 |    0.03 |
| VectorStoreArraySimpleBuffer |         16384 |       640.5 ns |     22.126 ns |     18.476 ns |       636.5 ns |  1.27 |    0.05 |
|                              |               |                |               |               |                |       |         |
|           **VectorStoreAligned** |        **131072** |     **9,865.0 ns** |    **199.512 ns** |    **279.687 ns** |     **9,767.2 ns** |  **1.00** |    **0.00** |
|       VectorStoreArrayMemPtr |        131072 |     9,637.7 ns |     94.004 ns |     83.332 ns |     9,645.3 ns |  0.97 |    0.03 |
|      VectorStoreArrayMemSafe |        131072 |     6,181.7 ns |    120.563 ns |    148.062 ns |     6,144.4 ns |  0.63 |    0.03 |
| VectorStoreArraySimpleBuffer |        131072 |     9,925.4 ns |    260.502 ns |    230.929 ns |     9,855.4 ns |  1.00 |    0.03 |
|                              |               |                |               |               |                |       |         |
|           **VectorStoreAligned** |       **1048576** |    **79,435.3 ns** |  **1,865.323 ns** |  **2,220.535 ns** |    **78,294.8 ns** |  **1.00** |    **0.00** |
|       VectorStoreArrayMemPtr |       1048576 |    98,353.8 ns |  2,720.589 ns |  2,271.815 ns |    97,951.3 ns |  1.24 |    0.03 |
|      VectorStoreArrayMemSafe |       1048576 |    79,803.5 ns |  1,712.943 ns |  3,000.081 ns |    78,598.9 ns |  1.01 |    0.06 |
| VectorStoreArraySimpleBuffer |       1048576 |    79,867.6 ns |  2,257.561 ns |  2,318.349 ns |    79,063.7 ns |  1.00 |    0.05 |
|                              |               |                |               |               |                |       |         |
|           **VectorStoreAligned** |       **2097152** |   **216,500.1 ns** |  **4,992.955 ns** | **14,164.183 ns** |   **212,591.0 ns** |  **1.00** |    **0.00** |
|       VectorStoreArrayMemPtr |       2097152 |   346,242.9 ns |  6,797.722 ns |  9,304.799 ns |   341,851.5 ns |  1.58 |    0.12 |
|      VectorStoreArrayMemSafe |       2097152 |   205,378.0 ns |  3,818.530 ns |  3,188.646 ns |   205,488.9 ns |  0.93 |    0.07 |
| VectorStoreArraySimpleBuffer |       2097152 |   228,231.7 ns |  4,517.376 ns | 10,736.022 ns |   225,121.4 ns |  1.06 |    0.09 |
|                              |               |                |               |               |                |       |         |
|           **VectorStoreAligned** |       **8388608** | **1,503,050.0 ns** | **28,335.402 ns** | **27,829.153 ns** | **1,490,845.2 ns** |  **1.00** |    **0.00** |
|       VectorStoreArrayMemPtr |       8388608 | 1,506,756.1 ns | 19,681.599 ns | 17,447.225 ns | 1,503,300.3 ns |  1.00 |    0.02 |
|      VectorStoreArrayMemSafe |       8388608 | 1,536,087.1 ns | 26,551.526 ns | 23,537.236 ns | 1,531,720.1 ns |  1.02 |    0.03 |
| VectorStoreArraySimpleBuffer |       8388608 | 1,541,513.7 ns | 32,303.380 ns | 30,216.602 ns | 1,536,127.9 ns |  1.02 |    0.03 |
