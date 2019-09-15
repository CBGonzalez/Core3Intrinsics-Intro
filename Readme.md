## Introduction to Core 3 Intrinsics in C#, with Benchmarks ##

Taking the new `System.Runtime.Intrinsics` namespace for a spin and comparing it to scalar `float` and `Vector<float>` operations.

#### Contents ####
- [Introduction to Intrinsics](#Intro)
- [First steps](#First)
- [Loading and storing data](#Load)
- [Basic Operations](#Basic)
- [Binding Commands](#Commands)
- [Binding with Data Entry and Validation](#Validation)
- [Binding for Dynamic Content](#DynCont)
- [Debugging Bindings](#Debugging)

#### <a name="Intro"/>Introduction to Intrinsics ####

The new functionality (available in Net Core 3.0 and beyond) under the `System.Runtime.Intrinsics` namespace will open up some the Intel processor intrinsics (see [Intel´s full guide here](https://software.intel.com/sites/landingpage/IntrinsicsGuide))) and a [Microsoft blog entry](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/) by Tanner on the subject. The coverage is not 100% but I imagine it will grow further as time passes. ARM processor support is in the future.

In a nutshell, the new functionality expands SIMD processing beyond what´s possible using `System.Numerics.Vector<T>` by adding dozens of new instructions.

#### <a name="First"/> First steps ####

You prepare your code by adding some `using`s to your code.
```C#
using System.Runtime.Intrinsics
using System.Runtime.Intrinsics.X86
```
`Intrinsics` contains the different new vector classes and structures ([Microsoft documentation](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics?view=netcore-3.0)): `Vector64<T>`, `Vector128<T>` and `Vector256<T>`. The number refers to the bit-length of the vector, as expected.

The classes offer functions for creating and transforming vectors: `Vector256.Create(1.0f)` creates a new `Vector256<float>`, with every component `float` initialized to `1.0f`, `Vector128.AsByte<float>(someVector128<float>)` creates a new vector128<byte>, casting the `float` values to `byte`.

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
            Vector64<byte> floatBytes = Vector64.AsByte(Vector64.Create(1.0f, -1.0f)); // doubleBytes = <0, 0, 128, 63, 0, 0, 128, 63>
        }
    }
}
```

`Intrinsics.X86` contains the SIMD namespaces, like SSE and AVX. It can be quite daunting (see [Microsoft´s documentation here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.x86?view=netcore-3.0)) since it does not contain any explanation of the functionality. For functions like `Add` it might not be necessary but for `Blend` the name itself is not necessarily enlightening (unless you are already familiar with Intel´s intrinsincs.)

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

As seen above, you can create vectors one-by-one using the `Create` function. Another possibility to use the (unsafe) `Avx.Loadxxx()` function.

If you look through the different `Load...` instructions available, you´ll notice that you have, for example, `LoadVector256(T*)` and `LoadAlignedVector256(T*)`.

> :warning: The "Aligned" part refers to memory alignment of pointer to the beginning of the <T> data: in order to use the `LoadAligned` version of the functions, your data needs to start at a specific boundary: for 256 bit vectors (32 bytes), the data ***needs*** to start at a location (pointer address) that is a multiple of 32 (for 128 bit vectors it needs to be aligned at 16 byte boundaries, but that´s the default for X64 bit systems anyhow, and that takes care of 64 bit vectors also). Failure to do so can result in a runtime ***general protection fault***.

The difference in performance for aligned vs. unaligned access can be tested with:

```C#
        [Benchmark]
        public unsafe void VectorStoreArrayMemPtr()
        {                        
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(dataMemory.Pointer, numberOfFloatItems));
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(storeMemory.Pointer, numberOfFloatItems));

            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];                
                i++;
            }
        }

        [Benchmark]
        public unsafe void VectorStoreArrayMemPtrUnaligned()
        {
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(data16Memory.Pointer, numberOfFloatItems));
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(store16Memory.Pointer, numberOfFloatItems));

            int i = 0;
            while (i < readMem.Length)
            {
                writeMem[i] = readMem[i];
                i++;
            }
        }
```

Using data arrays that fit into the L1 cache, I get the following results, using [BenchmarkDotNet](https://benchmarkdotnet.org/):

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview9-014004
  [Host]     : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview9-19423-09 (CoreCLR 4.700.19.42102, CoreFX 4.700.19.42104), 64bit RyuJIT
```

```
|                        Method | numberOfBytes |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD |
|------------------------------ |-------------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|
|        VectorStoreArrayMemPtr |         16384 |       401.3 ns |      8.049 ns |     12.998 ns |       397.5 ns |  1.00 |    0.07 |
|    VectorStoreUnalignedMemPtr |         16384 |       504.7 ns |     15.461 ns |     20.641 ns |       498.6 ns |  1.25 |    0.09 |
```

So, a 25% increase in time for unaligned data. Notice though that this difference will tend to disappear once your data is too large to fit the caches completely:

```
|                        Method | numberOfBytes |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD |
|------------------------------ |-------------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|
|       VectorStoreArrayMemSafe |       8388608 | 1,559,172.6 ns | 17,596.260 ns | 15,598.626 ns | 1,559,339.7 ns |  1.00 |    0.03 |
|    VectorStoreUnalignedMemPtr |       8388608 | 1,561,604.8 ns | 22,459.313 ns | 19,909.596 ns | 1,558,538.2 ns |  1.00 |    0.03 |

```
Since you´ll normally use intrinsics to process bigger chunks of data, instead of creating vectors one at a time it´s more efficient to prepare the data to be in a Span<Vectorxxx> form.

Imagining that you have a  big chunk `float` to process, you could use `MemoryMarshal.Cast` to prepare the data:

``` C#
            // The data to process; dataMemory.Pointer is a void* that points to the start of the data
            ReadOnlySpan<Vector256<float>> readMem = MemoryMarshal.Cast<float, Vector256<float>>(new ReadOnlySpan<float>(dataMemory.Pointer, numberOfFloatItems));

            // The array to receive the resulkts; storeMemory.Pointer is a void* that points to the start of the array
            Span<Vector256<float>> writeMem = MemoryMarshal.Cast<float, Vector256<float>>(new Span<float>(storeMemory.Pointer, numberOfFloatItems));
```

The `MemoryMarshal.Cast<float, Vector256<float>>()` bit casts `float` to `Vector256<float>`, no copying involved. You loop through the Span<Vector256<float>>> just as with any array, using the span´s `Length` property as a limit. The same works for storing the result: you just cast the array and can access the individual `float` in the original `Span<float>` or `float[]`.

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

#### <a name="Basic"/> Basic Math Operations ####

As mentioned above, `System.Runtime.Intrinsics.X86` contains the SSE, AVX etc. functionality.

#### <a name="Commands"/> Binding Commands ####

In the XAML definition of the pause `Button`
```XML.xaml
<Button x:Name="btnPause" Visibility="{Binding ShowPauseButton}" Width="16" Height="16" Background="#00DDDDDD" HorizontalAlignment="Right" VerticalAlignment="Top" Canvas.Top="1" Canvas.Right="32" Margin="0,0,0,0" BorderBrush="{x:Null}" ToolTip="Pause" Command="{Binding ButtonPauseCommand}" BorderThickness="0">
            <StackPanel Orientation="Horizontal">
                <Image Source="Images/Pause_Red_LT_16X.png"/>
            </StackPanel>
</Button>
```
the `Command="{Binding ButtonPauseCommand}"` part defines what happens when the button is clicked.

The `ButtonPauseCommand` points to an object implementing the [`ICommand Interface`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=netframework-4.8):

```C#
ICommand ButtonPauseCommand = new RelayCommand(param => PauseButtonClick(), param => CanPause);
```

`PauseButtonClick()` is just a simple function:

```C#
        private void PauseButtonClick()
        {
            animateOn = false;
            CanPause = false;
            ShowPauseButton = Visibility.Hidden;
            ShowResumeButton = Visibility.Visible;
            contentHandler.PauseRefresh();
        }
```
It will set some variables and command the underlying service to stop fetching headlines.

The real legwork happens in the `RelayCommand` class (from Josh Smith´s [Patterns - WPF Apps With The Model-View-ViewModel Design Pattern](https://msdn.microsoft.com/en-us/magazine/dd419663.aspx)). Instead of creating a full implementation of `ICommand` for every command we need, this class will implement it just once and generate our different commands´ logics. It is defined as:
```C#
    class RelayCommand : ICommand
    {        
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter ?? "<N/A>");
        }
    }
```
It´s constructor takes the function as the first argument. The second argument can point to a method defining if the function can be executed at any specific time.

In the case of the pause button it points to a `bool` value defining if there are items being scrolled.

#### <a name="Validation"/> Binding with Data Entry and Validation ####

A user-editable field in the UI that needs validation can also be handled through bindings. The news ticker has a window with configuration options, one of them expecting a numerical value.

![Validation-field](AdditionalFiles/Validation.png)

It is defined in XAML as

```XML.xaml
      <TextBox x:Name="refreshBox" HorizontalAlignment="Left" Height="25" Margin="181,33,0,0" TextWrapping="Wrap"  VerticalAlignment="Top" Width="38" HorizontalContentAlignment="Center" VerticalContentAlignment="Center" ToolTip="{x:Static localization:Resources.RefreshToolTip}">
            <TextBox.Text>
                <Binding Path="NetworkRefresh" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
                    <Binding.ValidationRules>
                        <ExceptionValidationRule/>
                    </Binding.ValidationRules>
                </Binding>
            </TextBox.Text>
        </TextBox>
```
Within the `Binding` declaration there is the `UpdateSourceTrigger` value, which determines when the underlying variable gets updated. `PropertyChanged` will trigger the update (and the validation) at each user input. Details and other possibilities are described in the docs for the [`UpdateSourceTrigger enum`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.updatesourcetrigger?view=netframework-4.8).

The `Mode="TwoWay"` part makes the binding work in both directions: a change in the underlying property is shown in the UI, and a change to the field in the UI is sent back to the variable.

The `Binding.ValidationRules` determine which rule to use. In the above code snippet the built-in `ExceptionValidationRule` is used: whenever the validation throws an exception, the `TextBox` will change its appearance (by default receiving a red margin). You can define your own [`ValidationRule`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.validationrule?view=netframework-4.8) if needed, for example to check for valid numeric or date ranges. Custom UI changes to the element can be defined in a [`Validation.ErrorTemplate`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.validation.errortemplate?view=netframework-4.8). (Microsoft has a simple, full [example on Github](https://github.com/Microsoft/WPF-Samples/tree/master/Data%20Binding/BindValidation) showing such a template)

The variable `NetworkRefresh` is defined as a `string`, backed by a `double`:
```C#
        public string NetworkRefresh
        {
            get => refresh.ToString("N1", System.Globalization.CultureInfo.CurrentCulture);

            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out double refr))
                {
                    refresh = refr;
                    NotifyPropertyChanged();
                }
                else
                {
                    throw new ApplicationException(Properties.Resources.ErrorMustBeNumeric);

                }
            }
        }
```
Notice that the setter throws an `Exception` if the parsing of the `string` to a `double` does not work, signaling the UI to notify the user.

#### <a name="DynCont"/> Binding for Dynamic Content ####

The ticker displays the headlines within a `Canvas`, as a series of `Button` elements. These scroll across the screen, from right to left in the current implementation. The underlying news service will refresh the headlines periodically and this needs to be reflected in the UI.

In order to achieve this, the buttons are created programmatically beforehand and their `Content` property is bound to a [`ObservableCollection<string>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1?view=netframework-4.8), which takes care of signaling the UI any time an object is added or modified.

```C#
private var headlines = new ObservableCollection<string>();
public ObservableCollection<string> Headlines => headlines;
[...]
            Button but;
            for(int i = 1; i < numberOfButtons; i++)
            {                
                headlines.Add(string.Empty);                
                but = new Button() { Width = buttonWidth, Height = 30, Background = Brushes.LightBlue, Name = "B1" };

                var contentBinding = new Binding($"Headlines[{i}]") { Mode = BindingMode.OneWay };
                but.SetBinding(Button.ContentProperty, contentBinding);

                NewsButtons.Add(but);
            }      
```
Initially, the headlines are created with a `string.Empty` `Content`. After refresh, the different string values are updated and automatically reflected in the button´s text, thanks to the magic of binding.

The binding is defined via `contentBinding = new Binding($"Headlines[{i}]") { Mode = BindingMode.OneWay };`: each button binds to a specific item in the Headlines list (Headlines[i]).

> :warning: Notice again the use of a `string` to define a binding. Beware of misspellings!

The `Mode = BindingMode.OneWay` part defines the binding to be *from* the data source *to* the UI. Any change within the UI does not reflect back to the data. See the docs for the [BindingMode enum](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.bindingmode?view=netframework-4.8) for more options.

The animation (scrolling) is achieved by binding the buttons `Canvas.LeftProperty` to an `ObservableCollection<double>`. This collection is updated several times a second in its own thread.

> :white_check_mark: Notice that one of the "hidden" advantages of using data binding is that you can change a UI element´s content or characteristics from a thread that is *not* the UI thread without cumbersome `BeginInvokes` to the UI `Dispatcher` by working on the data sources instead !

#### <a name="Debugging"/> Debugging Bindings ####

If using Visual Studio, you can [enable tracing for WPF](https://docs.microsoft.com/en-us/visualstudio/debugger/how-to-display-wpf-trace-information?view=vs-2019) and your Output window will show details of what´s going on behind the scenes. You can choose what level of information is displayed. The slight disadvantage is that *all* or *no* bindings are traced, so if you want full details, you´ll get a lot of text to wade through.

If you want to trace a specific binding with all its gory details, you use the above configuration to choose a low level of detail for all, then you include the following snippet in your XAML:
```XML.xaml
<Window
    [...]
    xmlns:diag="clr-namespace:System.Diagnostics;assembly=WindowsBase"
    [...]
</Window>
```
Within the `Binding` you´re interested in you do
```XML.xaml
    <Binding diag:PresentationTraceSources.Tracelevel=High [...] />
```

Alternatively, you can achieve the same in code:
```C#
Binding bind = new Binding() { [...]  };
System.Diagnostics.PresentationTraceSources.SetTraceLevel(bind, System.Diagnostics.PresentationTraceLevel.High);
```
The Output window in Visual Studio will display the details of the binding as they happen (errors and more).
