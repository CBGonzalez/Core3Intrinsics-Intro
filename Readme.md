## Introduction to Core 3 Intrinsics in C#, with Benchmarks ##

Taking the new `System.Runtime.Intrinsics` namespace for a spin and comparing it to scalar `float` and `Vector<float>` operations.

#### Contents ####
- [Introduction to Intrinsics](#Intro)
- [First steps](#First)
- [Loading and storing data](#Load)
- [Aligned vs. Unaligned Memory](#Aligned)
- [Dataset Sizes vs Caches](#Cache)
- [Basic Operations](#Basic)
- [Comparisons](#Compare)
- [What´s Missing?](#Missing)
- [Benchmark Results](#Benchmarks)

#### <a name="Intro"/>Introduction to Intrinsics ####

The new functionality (available in Net Core 3.0 and beyond) under the `System.Runtime.Intrinsics` namespace will open up some the Intel and AMD processor intrinsics (see [Intel´s full guide here](https://software.intel.com/sites/landingpage/IntrinsicsGuide))) and a [Microsoft blog entry](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/) by Tanner Gooding on the subject. The coverage is not 100% but I imagine it will grow further as time passes. ARM processor support is in the future.

In a nutshell, the new functionality expands SIMD processing beyond what´s possible using `System.Numerics.Vector<T>` by adding dozens of new instructions.

#### <a name="First"/> First steps ####

You prepare your code by adding some `using` statements:
```C#
using System.Runtime.Intrinsics
using System.Runtime.Intrinsics.X86
```
`Intrinsics` contains the different new vector classes and structures ([Microsoft documentation](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics?view=netcore-3.0)): `Vector64<T>`, `Vector128<T>` and `Vector256<T>`. The number refers to the bit-length of the vector, as expected.

The classes offer functions for creating and transforming vectors: `Vector256.Create(1.0f)` creates a new `Vector256<float>`, with every component `float` initialized to `1.0f`, `Vector128.AsByte<float>(someVector128<float>)` creates a new vector128<byte>, casting the `float` values to `byte`. Also, you can create vectors using `Create` and explicitly passing all elements.

```C#
using System.Runtime.Intrinsics;

namespace Core3Intrinsics
{
    public class Intro
    {
        public Intro()
        {
            Vector128<float> middleVector = Vector128.Create(1.0f);  // middleVector = <1,1,1,1>
            middleVector = Vector128.CreateScalar(-1.0f);  // middleVector = <-1,0,0,0>
            Vector64<byte> floatBytes = Vector64.AsByte(Vector64.Create(1.0f, -1.0f)); // floatBytes = <0, 0, 128, 63, 0, 0, 128, 63>
            Vector256<float> left = Vector256.Create(-1.0f, -2.0f, -3.0f, -4.0f, -5.0f, -6.0f, - 7.0f, -8.0f);
        }
    }
}
```

`Intrinsics.X86` contains the SIMD namespaces, like SSE and AVX. It can be quite daunting (see [Microsoft´s documentation here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.x86?view=netcore-3.0)) since it does not contain any explanation of the functionality. For functions like `Add` it might not be necessary but the `Blend` name itself is not necessarily enlightening (unless you are already familiar with Intel´s intrinsincs.)

All namespaces within `Intrinsics.X86` contain a static `IsSupported` `bool`: if `true` all is well and the platform supports the specific functionality (i. e. AVX2). If `false`, you are on your own, no software fallback is provided. If your code does not check for availability and happens to run on a hardware platform which does not support the functionality you are using, a `PlatformNotSupportedException` will be thrown at runtime.

These namespaces contain all the currently supported SIMD functions, like `Add`, `LoadVector256` and many more.

```C#
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3Intrinsics
{
    public class Intro
    {
        public Intro()
        {            
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
```

The [documentation](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.x86?view=netcore-3.0) contains the intrinsic function used by the processor (for `Add(Vector256<Single>, Vector256<Single>)` for example, the instruction is `__m256 _mm256_add_ps (__m256 a, __m256 b)`). This comes in handy in order to find the equivalent instruction in the [Intel guide](https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=884,287,2825,136&text=_mm256_add_ps):

```
__m256 _mm256_add_ps (__m256 a, __m256 b)
Synopsis
  __m256 _mm256_add_ps (__m256 a, __m256 b)
  #include <immintrin.h>
  Instruction: vaddps ymm, ymm, ymm
  CPUID Flags: AVX
Description
  Add packed single-precision (32-bit) floating-point elements in a and b, and store the results in dst.
Operation
  FOR j := 0 to 7
	  i := j*32
	  dst[i+31:i] := a[i+31:i] + b[i+31:i]
  ENDFOR
  dst[MAX:256] := 0

Performance
  | Architecture   | Latency | Throughput (CPI)
  | ---------------|---------|-----------------
  | Skylake        | 4       | 0.5
  | Broadwell      | 3       | 1
  | Haswell        | 3       | 1
  | Ivy Bridge     | 3       | 1
```

This gives you the exact description of the operation(s) being performed and also performance data (the "Latency" value is "is the number of processor clocks it takes for an instruction to have its data available for use by another instruction", the "Throughput" is "the number of processor clocks it takes for an instruction to execute or perform its calculations". See [Intels´ definition here](https://software.intel.com/en-us/articles/measuring-instruction-latency-and-throughput))

#### <a name="Load"/> Loading and storing data ####

##### Creating Vectors

As seen above, you can create vectors one-by-one using the various `Create` functions. Another possibility is to use the (unsafe) `Loadxxx()` functions.

Storing data can be achieved with Storexx.

``` C#
                double[] someDoubles = new double[] { 1.0, 3.0, -2.5, 7.5, 10.8, 0.33333 };
                double[] someResult = new double[someDoubles.Length];
                unsafe
                {
                    fixed (double* ptr = &someDoubles[1])
                    {
                        fixed (double* ptr2 = &someResult[0])
                        {
                            Vector256<double> res2 = Avx.LoadVector256(ptr); // res2 = <3, -2.5, 7.5, 10.8>
                            Avx.Store(ptr2, res2);
                        }
                    }
                }

```

You can also create a new vector by interleaving two others:
``` C#
                left = Vector256.Create(-1.0f, -2.0f, -3.0f, -4.0f, -50.0f, -60.0f, - 70.0f, -80.0f);
                right = Vector256.Create(1.0f, 2.0f, 3.0f, 4.0f, 50.0f, 60.0f, 70.0f, 80.0f);
                result = Avx.UnpackLow(left, right); // result = <-1, 1, -2, 2, -50, 50, -60, 60>
                result = Avx.UnpackHigh(left, right); // result = <-3, 3, -4, 4, -70, 70, -80, 80>
``` 
``` ini
R = UnpackLow(A, B)

     |------|------|------|------|------|------|------|------|
     |  A0  |  A1  |  A2  |  A3  |  A4  |  A5  |  A6  |  A7  |
     |------|------|------|------|------|------|------|------|
     |------|------|------|------|------|------|------|------|
     |  B0  |  B1  |  B2  |  B3  |  B4  |  B5  |  B6  |  B7  |
     |------|------|------|------|------|------|------|------|

        R0     R1     R2     R3     R4     R5     R6     R7
     |------|------|------|------|------|------|------|------|
     |  A0  |  B0  |  A1  |  B1  |  A4  |  B4  |  A5  |  B5  |
     |------|------|------|------|------|------|------|------|
```
##### Vectors from Arrays #####

Many times you´ll use the intrinsics for huge amounts of data, so a more practical approach to create vectors could be:

``` C#
        public float[] ProcessData(ref Span<float> input)
        {
            float[] results = new float[input.Length];
            Span<Vector256<float>> resultVectors = MemoryMarshal.Cast<float, Vector256<float>>(results);

            ReadOnlySpan<Vector256<float>> inputVectors = MemoryMarshal.Cast<float, Vector256<float>>(input);

            for(int i = 0; i < inputVectors.Length; i++)
            {
                resultVectors[i] = Avx.Sqrt(inputVectors[i]);                
            }

            return results;
        }
```

`System.Runtime.Interopservices.MemoryMarshal.Cast<fromType, toType>()` will cast values in place (i. e. no copying involved). At the end of the loop, the `results` array will automagically contain the individual floats from the vector operation (btw, the above example does not check if the `input` array fits neatly into `Vector256`, normally you´d need to process any remaining elements in a scalar way).

You can also go `unsafe` and loop through pointers, of course:

``` C#
        public unsafe float[] ProcessDataUnsafe(ref Span<float> input)
        {
            float[] results = new float[input.Length];
            fixed (float* inputPtr = &input[0])
            {
                float* inCurrent = inputPtr;
                fixed (float* resultPtr = &results[0])
                {
                    float* resEnd = resultPtr + results.Length;
                    float* resCurrent = resultPtr;
                    while (resCurrent < resEnd)
                    {
                        Avx.Store(resCurrent, Avx.Sqrt(Avx.LoadVector256(inCurrent)));
                        resCurrent += 8;
                        inCurrent += 8;
                    }
                }
            }
            return results;
        }
```
No performance difference on my machine, though.

#### <a name="Aligned"/> Aligned vs. Unaligned Memory

If you look through the different `Load...` instructions available, you´ll notice that you have, for example, `LoadVector256(T*)` and `LoadAlignedVector256(T*)`.

> :warning: The "Aligned" part refers to memory alignment of the pointer to the beginning of the <T> data: in order to use the `LoadAligned` version of the functions, your data needs to start at a specific boundary: for 256 bit vectors (32 bytes), the data ***needs*** to start at a location (pointer address) that is a multiple of 32 (for 128 bit vectors it needs to be aligned at 16 byte boundaries). Failure to do so can result in a runtime ***general protection fault***.

In the past, aligned data used to work much better that unaligned data, but modern processors don´t really care, as long as your data is aligned to the natural OS´s boundary in order to avoid stradling cache line boundaries or page boundaries (see [this comment by T. Gooding](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/#comment-2942), for example) 


##### Using intrinsics to copy memory? A disappointment... #####

Although moving data around using vectors seems pretty efficient, I was surprised to measure `System.Runtime.CompilerServices.Unsafe.CopyBlock(ref byte destination, ref byte source, uint byteCount)` as faster, independently of data size (i.e. even data far bigger than cache will be copied efficiently). Of course it´s unsafe in the sense that you need to know what you are doing (not `unsafe` though).

```
|                        Method | numberOfBytes |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD |
|------------------------------ |-------------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|
|              ScalarStoreBlock |         16384 |       306.1 ns |      8.539 ns |     12.246 ns |       302.8 ns |  1.00 |    0.00 |
|        VectorStoreArrayMemPtr |         16384 |       401.3 ns |      8.049 ns |     12.998 ns |       397.5 ns |  1.32 |    0.07 |

|              ScalarStoreBlock |       8388608 | 1,106,074.5 ns | 17,544.390 ns | 14,650.360 ns | 1,107,074.2 ns |  1.00 |    0.00 |
|        VectorStoreArrayMemPtr |       8388608 | 1,573,258.0 ns | 34,312.238 ns | 44,615.601 ns | 1,561,962.8 ns |  1.43 |    0.05 |

```
An impressive 32 - 43% advantage... It shows that a properly optimized scalar method (probably using some very smart assembly instructions) beats a naïve vectorization with ease.

#### <a name="Cache"/> Dataset Sizes vs Caches ####

Often overlooked, the size of your datasets may have an important impact on your processing times (apart from the obvious increase in elements): if all data fits in a processor core´s cache and only a few operations will be performed per data point, then memory acces times will be crucial and you´ll notice a non-linear increase in processing time vs. data size.

> :warning: In other words, when you measure your loop in order to determine your gains (if any!) from using intrinsics, it´s important to test with *data sizes close to the real data*. For huge data, test with arrays several times bigger than the available cache size, at least.

#### <a name="Basic"/> Basic Floating Point Math Operations ####

As mentioned above, `System.Runtime.Intrinsics.X86` contains the SSE, AVX etc. functionality. You can add, substract, multiply and divide all kinds of vectors. 

You also have `Sqrt` and `ReciprocalSqrt`, `Min` and `Max`, they all do what you expect.

Some more exotic operations are:

##### AddSubtract #####
*__m256d _mm256_addsub_pd (__m256d a, __m256d b)*

``` C#
                var left = Vector256.Create(-2.5f); // <-2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5>
                var right = Vector256.Create(5.0f); // <5, 5, 5, 5, 5, 5, 5, 5>
                Vector256<float> result = Avx.AddSubtract(left, right); // result = <-7.5, 2.5, -7.5, 2.5, -7.5, 2.5, -7.5, 2.5>
```


`AddSubtract` will *subtract* the even components (0, 2, ...) and *add* the odd ones (1, 3, ...). 

``` ini
|------|------|------|------|------|
|  A0  |  A1  |  A2  |  A3  | ...  |
|------|------|------|------|------|
    -      +      -      +     ...
|------|------|------|------|------|
|  B0  |  B1  |  B2  |  B3  | ...  |
|------|------|------|------|------|

```


##### DotProduct #####

*__m256 _mm256_dp_ps (__m256 a, __m256 b, const int imm8)*

The `Avx.DotProduct` is a bit out of the common:

``` C#
                left = Vector256.Create(-1.0f, -2.0f, -3.0f, -4.0f, -50.0f, -60.0f, - 70.0f, -80.0f);
                right = Vector256.Create(1.0f, 2.0f, 3.0f, 4.0f, 50.0f, 60.0f, 70.0f, 80.0f);   
                result = Avx.DotProduct(left, right, 0b1111_0001); // result = <-30, 0, 0, 0, -17400, 0, 0, 0>
```
This will actually create **2** products of 128 bit vectors: from the first four elements of `left` and `right`, stored on the first element of `result`, and the same for the right 4 elements, stored on the 5th element. In other words, it will perform a dot product on two 128 bit float vectors independently. It can be visualized as doing the dot product of 2 four float element vectors separately and simultaneously.

You can control which product is performed by using the 4 high order bits of the third parameter **in reverse order**: all ones means do all 4 products (on each 128 bit half). A value of `0b0001` would mean that only the **first** element´s products is performed, a value of `0b1010` will multiply first and third:

``` C#
                result = Avx.DotProduct(left, right, 0b1010_0001); // result = <-20, 0, 0, 0, -10000, 0, 0, 0>
```

If you think of vectors with x, y, z and w components, the order in which you turn the product on or off is thus (w, z, y, x).

The second half of the byte indicates where to store the dot product results, again in **reverse order**: `0001` means store the result in the first elements of each 128 bit vector.

``` ini
R = DotProduct(A, B, bitMask)

bit mask = b7 b6 b5 b4 0 0 0 1

         b4    b5     b6     b7      b4     b5     b6     b7
     |------|------|------|------||------|------|------|------|
     |  A0  |  A1  |  A2  |  A3  ||  A4  |  A5  |  A6  |  A7  |
     |------|------|------|------||------|------|------|------|
         *      *     *       *       *      *     *       *
     |------|------|------|------||------|------|------|------|
     |  B0  |  B1  |  B2  |  B3  ||  B4  |  B5  |  B6  |  B7  |
     |------|------|------|------||------|------|------|------|
         =      =     ...
         0       0                    0       0                                       
        or   +  or    ...            or   +  or    ...
       A0*B0   A1*B1                A4*B4   A5*B5     
      |__________________________||__________________________|
           						         
                stored in                  stored in
         |                           |
         |                           | 

         1     0      0       0       1      0      0     0  
     |------|------|------|------||------|------|------|------|     
     |  R0  |   0  |   0  |   0  ||  R4  |   0  |   0  |   0  |
     |------|------|------|------||------|------|------|------|
```

> :warning: You should do some benchmarking before using this instruction, its performance doesn´t seem to be too hot.

##### Floor, Ceiling #####
 These do what you expect:

``` C#
                var left = Vector256.Create(-2.5f); // <-2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5>
                var right = Vector256.Create(5.0f); // <5, 5, 5, 5, 5, 5, 5, 5>
                
                result = Avx.Floor(left);  // result = <-3, -3, -3, -3, -3, -3, -3, -3>  
                result = Avx.Ceiling(left); // result = <-2, -2, -2, -2, -2, -2, -2, -2>                
```

In order to have finer control you also have `RoundToNearestInteger` , `RoundToNegativeInfinity` etc.

##### Horizontal Add, Subtract #####

*__m256 _mm256_hadd_ps (__m256 a, __m256 b)*

*__m256 _mm256_hsub_ps (__m256 a, __m256 b)*

``` C#
                var left = Vector256.Create(-2.5f); // <-2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5, -2.5>
                var right = Vector256.Create(5.0f); // <5, 5, 5, 5, 5, 5, 5, 5>
                result = Avx.HorizontalAdd(left, right); // result = <-5, -5, 10, 10, -5, -5, 10, 10>
                result = Avx.HorizontalSubtract(left, right); // result = <0, 0, 0, 0, 0, 0, 0, 0>
```

`HorizontalAdd` will add element 0 and 1 from `left`, then elements 2 and 3. They get stored in elements 0 and 1 of `result`. The it goes on with the same for `right` and stores the results in elements 2 and 3 of `result`; then further...

``` ini  
R = HorizontalAdd(A, B)       
     |------|------|------|------|------|------|------|------|
     |  A0  |  A1  |  A2  |  A3  |  A4  |  A5  |  A6  |  A7  |
     |------|------|------|------|------|------|------|------|
     |------|------|------|------|------|------|------|------|
     |  B0  |  B1  |  B2  |  B3  |  B4  |  B5  |  B6  |  B7  |
     |------|------|------|------|------|------|------|------|

           R0         R1         R2         R3         R4         R5         R6         R7
     |----------|----------|----------|----------|----------|----------|----------|----------|     
     |  A0 + A1 |  A2 + A3 |  B0 + B1 |  B2 + B3 |  A4 + A5 |  A6 + A7 |  B4 + B5 |  B6 + B7 |
     |----------|----------|----------|----------|----------|----------|----------|----------|
  
```

##### FMA - Fused Multiply #####

*__m256 _mm256_fmadd_ps* etc.

``` C#
                if (Fma.IsSupported)
                {
                    var resultFma = Fma.MultiplyAdd(left, right, other); // = left * right + other for each element
                    resultFma = Fma.MultiplyAddNegated(left, right, other); // = -(left * right + other) for each element
                    resultFma = Fma.MultiplySubtract(left, right, other); // = left * right - other for each element
                    Fma.MultiplyAddSubtract(left, right, other); // even elements (0, 2, ...) like MultiplyAdd, odd elements like MultiplySubtract 
                }
```
These instructions will combine multiplies with add or substract in several variants.

#### <a name="Compare"/> Comparisons #### 

There are several intrinsics to compare values.

##### Vector results

`Avx.Compare(vector a, vector b, flag)` will compare both vectors according to the `FloatComparisonMode` flag given.
``` C#
                left = Vector256.Create(-1.0f, 3.0f, -3.0f, 4.0f, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var compareResult = Avx.Compare(left, right, FloatComparisonMode.OrderedGreaterThanNonSignaling); // compareResult = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
``` 
`FloatComparisonMode.OrderedGreaterThanNonSignaling` will compare if elements in `left` are greater than elements in `right`. If the comparison is `false`, the result vector will have a zero in that position. If `true` the position will be occupied by a value of all bits set to 1 (which results in `NaN` for `float` and `double`).

> The `Ordered...` part of the flag´s name refers to how `NaN` in the vectors are treated, the `...NonSignaling` means to not throw exceptions when NaNs occur, although I am not really sure how this works yet [TO BE CONTINUED].

Once you have the comparison result, there are several things you can do with it:

``` C#
                left = Vector256.Create(-1.0f, 3.0f, -3.0f, 4.0f, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var compareResult = Avx.Compare(left, right, FloatComparisonMode.OrderedGreaterThanNonSignaling); // compareResult = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
                int res = Avx.MoveMask(compareResult); // res = 0b10101010 = 0xAA = 170

                if(int > 0)
                {
                   // At least one comparison is true, do something
                }
```

`MoveMask` will create an `int` which bits indicate the elements which are `true` (in reality, it will copy each element´s highes order bit, which comes down to the same). The `int` will list the elements **in reverse order**.

If you don´t need to know which element satisfies the comparison but just know if all did, you can do:

``` C#
                left = Vector256.Create(-1.0f, 3.0f, -3.0f, 4.0f, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var compareResult = Avx.Compare(left, right, FloatComparisonMode.OrderedGreaterThanNonSignaling); // compareResult = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
                bool areAllTrue = !Avx.TestZ(compareResult, compareResult); // areAllTrue = false

                if(!areAllTrue)
                {
                   // At least one comparison is false, do something
                }
```

You can also use the resulting vector to selectively load vector elements:

``` C#
                left = Vector256.Create(-1.0f, 3.0f, -3.0f, 4.0f, -50.0f, 60.0f, -70.0f, 80.0f);
                right = Vector256.Create(0.0f, 2.0f, 3.0f, 2.0f, 50.0f, -60.0f, 70.0f, -80.0f);
                var mask = Avx.Compare(left, right, FloatComparisonMode.OrderedGreaterThanNonSignaling); // mask = <0, NaN, 0, NaN, 0, NaN, 0, NaN>
                Vector256<float> mixed = Avx.BlendVariable(left, right, mask); //  mixed = <-1, 2, -3, 2, -50, -60, -70, -80>
```

For each element in the third parameter (`mask`), `BlendVariable` will pick the correspondent element from the **second** vector (`right` in the above snippet) if the mask´s value is **`true`**, from the first vector otherwise.

In the above snippet, `left`[0] = `-1.0f`, `right`[0] = `0.0f`. The mask is `0` (false) at this position, so the result vector´s first position gets the value from the **first** vector: `-1.0f`.

##### Scalar Results #####

As mentioned above, there are some intrinsics to compare values that return a scalar (`int` or `bool`): `TestZ`, `TestC` etc and `MoveMask`.

#### <a name="Missing"/> What´s Missing? #### 

There are no [trigonometric functions](https://software.intel.com/sites/landingpage/IntrinsicsGuide/#cats=Trigonometry) as yet: cosine, sine etc. are all missing. Maybe some others, but that´s the category that caught my eye.

#### <a name="benchmarks"/> Benchmark Results ####

Some benchmarks, with small data sizes (i. e. the data should fit into L2 cache) and larger sizes (i. e. 10 x L3 cachesize ) on my machine.

##### FMA #####


A simple scalar loop:

``` C#
        [BenchmarkCategory("MultiplyAdd"), Benchmark(Baseline = true)]
        public unsafe void MultiplyAddScalarFloat()
        {
            var sp1 = new ReadOnlySpan<float>(data, 0, numberOfFloatItems);
            var sp12 = new ReadOnlySpan<float>(data2, 0, numberOfFloatItems);
            var sp13 = new ReadOnlySpan<float>(data3, 0, numberOfFloatItems);
            var sp2 = new Span<float>(result, 0, numberOfFloatItems);

            for (int i = 0; i < sp1.Length; i++)
            {
                sp2[i] = sp1[i] * sp12[i] + sp13[i];
            }
        }
```

The same using `Fma`:

``` C#
        [BenchmarkCategory("MultiplyAdd"), Benchmark]
        public unsafe void FmaMultiplyAddvector256Float()
        {            
            ReadOnlySpan<Vector256<float>> d1 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d2 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data2, 0, numberOfFloatItems));
            ReadOnlySpan<Vector256<float>> d3 = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(data3, 0, numberOfFloatItems));
            Span<Vector256<float>> r = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(result, 0, numberOfFloatItems));

            for (int i = 0; i < d1.Length; i++)
            {
                r[i] = Fma.MultiplyAdd(d1[i], d2[i], d3[i]);
            }
        }
```
Comparing both gives:

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-rc1-014190
  [Host]     : .NET Core 3.0.0-rc1-19456-20 (CoreCLR 4.700.19.45506, CoreFX 4.700.19.45604), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-rc1-19456-20 (CoreCLR 4.700.19.45506, CoreFX 4.700.19.45604), 64bit RyuJIT


```
|                        Method | ParamCacheSizeBytes |         Mean |       Error |      StdDev | Ratio | RatioSD |
|------------------------------ |-------------------- |-------------:|------------:|------------:|------:|--------:|
|        **MultiplyAddScalarFloat** |              **262144** |    **20.128 us** |   **0.5597 us** |   **0.8377 us** |  **1.00** |    **0.00** |
|  FmaMultiplyAddvector256Float |              262144 |     6.750 us |   0.1338 us |   0.1186 us |  0.33 |    0.02 |
|                               |                     |              |             |             |       |         |
|        **MultiplyAddScalarFloat** |            **41943040** | **5,208.768 us** | **103.2312 us** | **257.0815 us** |  **1.00** |    **0.00** |
|  FmaMultiplyAddvector256Float |            41943040 | 4,021.671 us |  75.5671 us |  70.6856 us |  0.78 |    0.04 |


As expected for small number of operations, the memory access times take their tolls: only a 22% time reduction for larger data sizes with vector intrinsics.

If we perform 3 FMA operations per step in the loop on the other hand, we get a consistent 2.2x speedup (see the source code for implementation of the test):

|                    Method | ParamCacheSizeBytes |        Mean |       Error |     StdDev |      Median | Ratio | RatioSD |
|-------------------------- |-------------------- |------------:|------------:|-----------:|------------:|------:|--------:|
|    **ScalarFloatMultipleOps** |              **262144** |    **41.23 us** |   **1.4217 us** |   **1.187 us** |    **40.87 us** |  **1.00** |    **0.00** |
| Vector256FloatMultipleOps |              262144 |    19.35 us |   0.5057 us |   1.491 us |    19.01 us |  0.45 |    0.04 |
|                           |                     |             |             |            |             |       |         |
|    **ScalarFloatMultipleOps** |            **41943040** | **8,848.52 us** | **256.5261 us** | **748.299 us** | **9,073.76 us** |  **1.00** |    **0.00** |
| Vector256FloatMultipleOps |            41943040 | 4,093.59 us |  80.3949 us |  95.704 us | 4,046.29 us |  0.45 |    0.04 |





