using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Core3IntrinsicsBenchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
    public class Mandelbrot
    {
        //[Params(4 * 1024 * 1024, 16 * 1024 * 1024)] //L3, 4 * L3
        public int TotalBytes {get; set; }

        public int numberOfTasks = 2;
        const float LEFT_X = -2.5f;
        const float RIGHT_X = 1.0f;
        const float TOP_Y = 1.0f;
        const float BOTT_Y = -1.0f;
        const float RATIO_Y_X = (TOP_Y - BOTT_Y) / (RIGHT_X - LEFT_X);

        int resolutionX, resolutionY;
        readonly float ratioy_x = RATIO_Y_X;
        public Memory<float> results;
        public int SizeInBytes => numberOfPoints * sizeof(float);
        Memory<float> xPoints, yPoints;
        int numberOfPoints;

        [GlobalSetup]
        public void GlobalSetup()
        {            
            resolutionX = 1920;            
            resolutionY = (int)MathF.Floor(resolutionX * ratioy_x);            
            float STEP_X = (RIGHT_X - LEFT_X) / resolutionX;
            float STEP_Y = (TOP_Y - BOTT_Y) / resolutionY;
            
            numberOfPoints = resolutionX * resolutionY;
            results = new float[numberOfPoints];
            xPoints = new float[resolutionX];
            yPoints = new float[resolutionY];
            for(int i = 0; i < resolutionX; i++)
            {
                xPoints.Span[i] = LEFT_X + i * STEP_X;                
            }
            for (int i = 0; i < resolutionY; i++)
            {                
                yPoints.Span[i] = TOP_Y - i * STEP_Y;
            }
        }

        [Benchmark(Baseline = true)]
        public void FloatMandel()
        {
            float currentY;
            float currentX;
            int countX = 0, countY = 0;
            int maxInter = 256;
            int inter;
            float zSquare, xSquare, ySquare, x, y;
            ReadOnlySpan<float> ySpan = yPoints.Span;
            ReadOnlySpan<float> xSpan = xPoints.Span;
            Span<float> res = results.Span;
            int floatCounter = 0;
            float q;
            float one16 = 1.0f / 16.0f;
            while (countY < resolutionY)
            {
                currentY = ySpan[countY];
                while (countX < resolutionX)
                {
                    currentX = xSpan[countX];                    
                    zSquare = xSquare = ySquare = 0.0f;
                    inter = 0;
                    bool goOn;// = true;
                    float temp = (currentX - 0.25f);
                    float temp1 = currentY * currentY;
                    q = temp * temp + temp1;
                    goOn = (q * (q + (temp)) > 0.25f * temp1); // out of cardioid? see https://en.wikipedia.org/wiki/Mandelbrot_set#Cardioid_/_bulb_checking
                    if (goOn)
                    {
                        goOn = (currentX + 1.0f) * (currentX + 1.0f) + temp1 > one16; // out of period-2 bulb?
                        if (!goOn)
                        {
                            inter = 255;
                        }
                    }
                    
                    while (goOn && inter < maxInter)
                    {
                        x = xSquare - ySquare + currentX;
                        y = zSquare - ySquare - xSquare + currentY;
                        xSquare = x * x;
                        ySquare = y * y;
                        zSquare = (x + y) * (x + y);
                        goOn = xSquare + ySquare <= 4.0f;

                        inter = goOn ? inter + 1 : inter;
                    }                    
                    res[floatCounter] = inter;                    
                    countX++;
                    floatCounter++;                    
                }                
                countX = 0;
                countY++;
            }
        }

        [Benchmark]
        public unsafe void Vector256Mandel()
        {                        
            int countX = 0, countY = 0;
            int maxInter = 256;
            int inter;
            ReadOnlySpan<float> ySpan = yPoints.Span;
            ReadOnlySpan<Vector256<float>> xSpan = MemoryMarshal.Cast<float, Vector256<float>>(xPoints.Span);
            Span<Vector256<float>> res = MemoryMarshal.Cast<float, Vector256<float>>(results.Span);
            int resVectorNumber = 0;

            Vector256<float> xVec, yVec;
            Vector256<float> zeroVec = Vector256<float>.Zero;
            var oneVec = Vector256.Create(1.0f);
            var fourVec = Vector256.Create(4.0f);
            var one4Vec = Vector256.Create(0.25f);
            var one16Vec = Vector256.Create(1.0f/16.0f);
            Vector256<float> qVec;
            Vector256<float> test;

            while (countY < ySpan.Length)
            {
                var currYVec = Vector256.Create(ySpan[countY]);
                while (countX < xSpan.Length)
                {
                    Vector256<float> currXVec = xSpan[countX];
                    Vector256<float> xSquVec = zeroVec;
                    Vector256<float> ySquVec = zeroVec;
                    Vector256<float> zSquVec = zeroVec;
                    Vector256<float> interVec = zeroVec;
                    Vector256<float> sumVector;
                    
                    inter = 0;
                    bool goOn;
                    Vector256<float> temp = Avx.Subtract(currXVec, one4Vec);
                    Vector256<float> temp1 = Avx.Multiply(currYVec, currYVec);
                    qVec = Avx.Add(Avx.Multiply(temp, temp), temp1);
                    Vector256<float> temp2 = Avx.Multiply(qVec, Avx.Add(qVec, temp));
                    test = Avx.Compare(temp2, Avx.Multiply(one4Vec, temp1), FloatComparisonMode.OrderedGreaterThanNonSignaling);
                    goOn = (Avx.MoveMask(test) > 0);
                    if(goOn)
                    {                        
                        temp2 = Avx.Add(currXVec, oneVec);
                        temp = Avx.Add(Avx.Multiply(temp2, temp2), temp1);
                        test = Avx.Compare(temp, one16Vec, FloatComparisonMode.OrderedGreaterThanNonSignaling);
                        goOn = Avx.MoveMask(test) > 0;
                        if (!goOn)
                        {
                            interVec = Vector256.Create(255.0f); // make all point = maximum value
                        }
                    }                    
                    while (goOn)
                    {
                        xVec = Avx.Add(Avx.Subtract(xSquVec, ySquVec), currXVec);
                        yVec = Avx.Add(Avx.Subtract(Avx.Subtract(zSquVec, ySquVec), xSquVec), currYVec);
                        xSquVec = Avx.Multiply(xVec, xVec);
                        ySquVec = Avx.Multiply(yVec, yVec);
                        temp = Avx.Add(xVec, yVec);
                        zSquVec = Avx.Multiply(temp, temp);
                        test = Avx.Compare(Avx.Add(xSquVec, ySquVec), fourVec, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling); // <= 4.0?
                        sumVector = Avx.BlendVariable(zeroVec, oneVec, test);

                        goOn = (Avx.MoveMask(test) > 0) & (inter < maxInter); //any of the values still alive, and inter still below cutoff value? 
                        if (goOn)
                        {
                            interVec = Avx.Add(interVec, sumVector);
                        }
                        inter = goOn ? inter + 1 : inter;
                    }                    
                    res[resVectorNumber] = interVec;
                    resVectorNumber++;                              
                    countX++;                    
                }
                countX = 0;                                
                countY++;                
            }
        }
    }
}
