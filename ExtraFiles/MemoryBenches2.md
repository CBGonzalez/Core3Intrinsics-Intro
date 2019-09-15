``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview9-014004
  [Host]     : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT


```
|                          Method | numberOfBytes |           Mean |         Error |        StdDev | Ratio | RatioSD |
|-------------------------------- |-------------- |---------------:|--------------:|--------------:|------:|--------:|
|                **ScalarStoreBlock** |         **16384** |       **298.5 ns** |      **5.924 ns** |      **9.047 ns** |  **1.00** |    **0.00** |
|          VectorStoreArrayMemPtr |         16384 |       394.1 ns |     10.456 ns |     16.885 ns |  1.32 |    0.06 |
| VectorStoreArrayMemPtrUnaligned |         16384 |       495.0 ns |      9.477 ns |     10.140 ns |  1.66 |    0.07 |
|                                 |               |                |               |               |       |         |
|                **ScalarStoreBlock** |        **131072** |     **6,225.2 ns** |    **116.328 ns** |    **103.122 ns** |  **1.00** |    **0.00** |
|          VectorStoreArrayMemPtr |        131072 |     6,772.1 ns |     77.929 ns |     65.074 ns |  1.09 |    0.02 |
| VectorStoreArrayMemPtrUnaligned |        131072 |     7,245.7 ns |    130.736 ns |    115.894 ns |  1.16 |    0.03 |
|                                 |               |                |               |               |       |         |
|                **ScalarStoreBlock** |       **1048576** |    **67,515.4 ns** |  **2,549.673 ns** |  **2,618.326 ns** |  **1.00** |    **0.00** |
|          VectorStoreArrayMemPtr |       1048576 |    80,868.2 ns |  1,569.923 ns |  1,928.007 ns |  1.20 |    0.05 |
| VectorStoreArrayMemPtrUnaligned |       1048576 |    83,708.5 ns |  1,995.286 ns |  2,134.934 ns |  1.24 |    0.05 |
|                                 |               |                |               |               |       |         |
|                **ScalarStoreBlock** |       **2097152** |   **189,619.0 ns** |  **7,155.162 ns** | **21,097.157 ns** |  **1.00** |    **0.00** |
|          VectorStoreArrayMemPtr |       2097152 |   271,783.7 ns |  5,376.659 ns | 11,914.305 ns |  1.41 |    0.17 |
| VectorStoreArrayMemPtrUnaligned |       2097152 |   274,970.6 ns |  5,310.311 ns |  5,453.298 ns |  1.44 |    0.15 |
|                                 |               |                |               |               |       |         |
|                **ScalarStoreBlock** |       **8388608** | **1,105,687.5 ns** | **10,205.821 ns** |  **8,522.323 ns** |  **1.00** |    **0.00** |
|          VectorStoreArrayMemPtr |       8388608 | 1,573,145.8 ns | 31,795.047 ns | 29,741.107 ns |  1.42 |    0.02 |
| VectorStoreArrayMemPtrUnaligned |       8388608 | 1,568,842.2 ns | 28,942.750 ns | 27,073.066 ns |  1.42 |    0.03 |
