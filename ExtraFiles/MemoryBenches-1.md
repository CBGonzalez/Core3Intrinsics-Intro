``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview9-014004
  [Host]     : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT


```
|                        Method | numberOfBytes |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD |
|------------------------------ |-------------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|
|           **ScalarStoreUnrolled** |         **16384** |     **2,292.3 ns** |     **60.087 ns** |     **50.176 ns** |     **2,284.6 ns** |  **7.52** |    **0.30** |
|              ScalarStoreBlock |         16384 |       306.1 ns |      8.539 ns |     12.246 ns |       302.8 ns |  1.00 |    0.00 |
|            VectorStoreAligned |         16384 |       493.2 ns |      9.847 ns |     12.453 ns |       493.2 ns |  1.60 |    0.06 |
|        VectorStoreArrayMemPtr |         16384 |       401.3 ns |      8.049 ns |     12.998 ns |       397.5 ns |  1.32 |    0.07 |
|       VectorStoreArrayMemSafe |         16384 |       473.3 ns |      9.507 ns |     13.327 ns |       470.7 ns |  1.55 |    0.08 |
|          VectorStoreUnaligned |         16384 |       577.2 ns |     10.582 ns |      9.381 ns |       576.0 ns |  1.89 |    0.07 |
|    VectorStoreUnalignedMemPtr |         16384 |       504.7 ns |     15.461 ns |     20.641 ns |       498.6 ns |  1.65 |    0.09 |
| VectorStoreUnalignedToAligned |         16384 |       492.7 ns |      9.763 ns |     16.311 ns |       485.8 ns |  1.61 |    0.08 |
|                               |               |                |               |               |                |       |         |
|           **ScalarStoreUnrolled** |        **131072** |    **18,656.4 ns** |    **343.541 ns** |    **321.348 ns** |    **18,589.3 ns** |  **3.02** |    **0.06** |
|              ScalarStoreBlock |        131072 |     6,185.0 ns |     77.250 ns |     64.508 ns |     6,174.3 ns |  1.00 |    0.00 |
|            VectorStoreAligned |        131072 |     6,873.3 ns |     65.477 ns |     54.676 ns |     6,880.6 ns |  1.11 |    0.02 |
|        VectorStoreArrayMemPtr |        131072 |     6,653.6 ns |    141.340 ns |    132.209 ns |     6,610.1 ns |  1.08 |    0.03 |
|       VectorStoreArrayMemSafe |        131072 |     6,931.2 ns |    138.136 ns |    282.176 ns |     6,822.8 ns |  1.13 |    0.06 |
|          VectorStoreUnaligned |        131072 |     7,556.5 ns |    114.427 ns |     89.337 ns |     7,537.2 ns |  1.22 |    0.02 |
|    VectorStoreUnalignedMemPtr |        131072 |     7,319.7 ns |    145.018 ns |    221.457 ns |     7,239.3 ns |  1.19 |    0.04 |
| VectorStoreUnalignedToAligned |        131072 |     6,928.4 ns |    138.061 ns |    141.779 ns |     6,892.1 ns |  1.12 |    0.03 |
|                               |               |                |               |               |                |       |         |
|           **ScalarStoreUnrolled** |       **1048576** |   **159,693.3 ns** |  **2,764.505 ns** |  **2,308.487 ns** |   **159,156.2 ns** |  **2.43** |    **0.07** |
|              ScalarStoreBlock |       1048576 |    65,713.1 ns |  1,277.124 ns |  1,132.137 ns |    65,699.8 ns |  1.00 |    0.00 |
|            VectorStoreAligned |       1048576 |    85,778.4 ns |  2,106.262 ns |  5,975.114 ns |    83,181.5 ns |  1.31 |    0.10 |
|        VectorStoreArrayMemPtr |       1048576 |    78,964.1 ns |  1,518.257 ns |  1,624.518 ns |    78,922.6 ns |  1.20 |    0.03 |
|       VectorStoreArrayMemSafe |       1048576 |    80,763.9 ns |  1,389.509 ns |  1,160.303 ns |    80,709.0 ns |  1.23 |    0.03 |
|          VectorStoreUnaligned |       1048576 |    84,741.3 ns |  1,680.962 ns |  2,185.725 ns |    84,040.2 ns |  1.29 |    0.04 |
|    VectorStoreUnalignedMemPtr |       1048576 |    82,595.5 ns |  1,816.659 ns |  2,019.212 ns |    82,142.8 ns |  1.26 |    0.04 |
| VectorStoreUnalignedToAligned |       1048576 |    86,209.3 ns |  1,984.263 ns |  5,693.224 ns |    85,122.7 ns |  1.30 |    0.09 |
|                               |               |                |               |               |                |       |         |
|           **ScalarStoreUnrolled** |       **2097152** |   **386,240.6 ns** |  **7,648.523 ns** | **19,188.650 ns** |   **381,202.7 ns** |  **2.26** |    **0.11** |
|              ScalarStoreBlock |       2097152 |   171,998.1 ns |  3,435.604 ns |  5,142.251 ns |   170,366.1 ns |  1.00 |    0.00 |
|            VectorStoreAligned |       2097152 |   250,602.9 ns |  3,544.961 ns |  2,960.203 ns |   250,186.1 ns |  1.45 |    0.05 |
|        VectorStoreArrayMemPtr |       2097152 |   253,581.1 ns |  5,065.490 ns |  9,003.903 ns |   251,693.9 ns |  1.48 |    0.06 |
|       VectorStoreArrayMemSafe |       2097152 |   254,647.4 ns |  5,565.014 ns | 10,034.868 ns |   251,608.8 ns |  1.49 |    0.07 |
|          VectorStoreUnaligned |       2097152 |   258,129.5 ns |  5,127.175 ns |  7,018.136 ns |   256,494.3 ns |  1.50 |    0.06 |
|    VectorStoreUnalignedMemPtr |       2097152 |   259,253.1 ns |  5,207.113 ns |  8,408.518 ns |   257,269.9 ns |  1.51 |    0.07 |
| VectorStoreUnalignedToAligned |       2097152 |   268,083.3 ns |  5,350.387 ns | 14,736.521 ns |   270,760.6 ns |  1.55 |    0.10 |
|                               |               |                |               |               |                |       |         |
|           **ScalarStoreUnrolled** |       **8388608** | **1,792,974.9 ns** | **34,861.894 ns** | **59,198.142 ns** | **1,773,807.8 ns** |  **1.64** |    **0.07** |
|              ScalarStoreBlock |       8388608 | 1,106,074.5 ns | 17,544.390 ns | 14,650.360 ns | 1,107,074.2 ns |  1.00 |    0.00 |
|            VectorStoreAligned |       8388608 | 1,564,931.4 ns | 38,160.539 ns | 37,478.752 ns | 1,549,061.2 ns |  1.42 |    0.04 |
|        VectorStoreArrayMemPtr |       8388608 | 1,573,258.0 ns | 34,312.238 ns | 44,615.601 ns | 1,561,962.8 ns |  1.43 |    0.05 |
|       VectorStoreArrayMemSafe |       8388608 | 1,559,172.6 ns | 17,596.260 ns | 15,598.626 ns | 1,559,339.7 ns |  1.41 |    0.03 |
|          VectorStoreUnaligned |       8388608 | 1,541,325.1 ns | 18,699.861 ns | 14,599.621 ns | 1,541,280.2 ns |  1.39 |    0.02 |
|    VectorStoreUnalignedMemPtr |       8388608 | 1,561,604.8 ns | 22,459.313 ns | 19,909.596 ns | 1,558,538.2 ns |  1.41 |    0.03 |
| VectorStoreUnalignedToAligned |       8388608 | 1,546,770.0 ns | 19,669.857 ns | 15,356.930 ns | 1,543,577.9 ns |  1.40 |    0.02 |
