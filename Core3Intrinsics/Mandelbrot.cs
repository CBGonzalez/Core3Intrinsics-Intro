using System;
using System.Runtime.Intrinsics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Core3Intrinsics
{
    public class Mandelbrot
    {
        readonly int TOTALBYTES = 16 * 1024 * 1024;//4 * 1024 * 1024;
        public int numberOfTasks = 1;

        const float LEFT_X = -2.5f;
        const float RIGHT_X = 1.0f;
        const float TOP_Y = 1.0f;
        const float BOTT_Y = -1.0f;
        
        int resolutionX, resolutionY;
        readonly float ratioy_x = (TOP_Y - BOTT_Y) / (RIGHT_X - LEFT_X);
        float STEP_X;
        float STEP_Y;
        public Memory<float> results, results2, testValue1, testValue2;
        public int SizeInBytes => numberOfPoints * sizeof(float);
        Memory<float> xPoints, yPoints;
        int numberOfPoints;
                                  
        public void FloatMandel()
        {
            int floatL3Size = TOTALBYTES / sizeof(float);
            resolutionX = (int)MathF.Floor(MathF.Sqrt(floatL3Size * ratioy_x));
            if (resolutionX % 8 != 0)
            {
                resolutionX -= resolutionX % 8;
            }
            resolutionY = (int)MathF.Floor(resolutionX * ratioy_x);
            if (resolutionY % 8 != 0)
            {
                resolutionY -= resolutionY % 8;
            }
            STEP_X = (RIGHT_X - LEFT_X) / resolutionX;
            STEP_Y = STEP_X; // ratioy_x * STEP_X; Bug from reddit comment
            numberOfPoints = resolutionX * resolutionY;
            if(numberOfPoints % 8 != 0)
            {
                numberOfPoints += numberOfPoints % 8;
            }
            results = new float[numberOfPoints];
            testValue1 = new float [numberOfPoints];
            testValue2 = new float [numberOfPoints];

            xPoints = new float[resolutionX];
            yPoints = new float[resolutionY];
            for (int i = 0; i < resolutionX; i++)
            {
                xPoints.Span[i] = LEFT_X + i * STEP_X;
            }
            for (int i = 0; i < resolutionY; i++)
            {
                yPoints.Span[i] = TOP_Y - i * STEP_Y;
            }

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
            while (countY < resolutionY)
            {
                
                currentY = ySpan[countY];                            
                while (countX < resolutionX)
                {
                    
                    currentX = xSpan[countX];
                    zSquare = xSquare = ySquare = 0.0f;
                    inter = 0;
                    bool goOn;
                    while (xSquare + ySquare <= 4.0f && inter < maxInter)
                    {
                        x = xSquare - ySquare + currentX;
                        y = zSquare - ySquare - xSquare + currentY;
                        xSquare = x * x;
                        ySquare = y * y;
                        zSquare = (x + y) * (x + y);
                        goOn = xSquare + ySquare <= 4.0f;
                        
                        inter = goOn ? inter + 1 : inter;
                    }
                    //res[countY * resolutionX + countX] = inter;
                    res[floatCounter] = inter;
                    testValue1.Span[floatCounter] = xSquare + ySquare;
                    countX++;
                    floatCounter++;                    
                }                
                countX = 0;
                countY++;                               
            }
        }
        
        public unsafe void Vector256Mandel()
        {
            int floatL3Size = TOTALBYTES / sizeof(float);
            resolutionX = (int)MathF.Floor(MathF.Sqrt(floatL3Size * ratioy_x));
            if (resolutionX % 8 != 0)
            {
                resolutionX -= resolutionX % 8;
            }
            resolutionY = (int)MathF.Floor(resolutionX * ratioy_x);
            if (resolutionY % 8 != 0)
            {
                resolutionY -= resolutionY % 8;
            }
            STEP_X = (RIGHT_X - LEFT_X) / resolutionX;
            STEP_Y = STEP_X; // ratioy_x * STEP_X; Bug from reddit comment
            numberOfPoints = resolutionX * resolutionY;
            results2 = new float[numberOfPoints];

            xPoints = new float[resolutionX];
            yPoints = new float[resolutionY];
            for (int i = 0; i < resolutionX; i++)
            {
                xPoints.Span[i] = LEFT_X + i * STEP_X;                
            }
            for (int i = 0; i < resolutionY; i++)
            {                
                yPoints.Span[i] = TOP_Y - i * STEP_Y;
            }

            int countX = 0, countY = 0;
            int maxInter = 256;
            int inter;
            ReadOnlySpan<float> ySpan = yPoints.Span;// MemoryMarshal.Cast<float, Vector256<float>>(yPoints.Span);
            ReadOnlySpan<Vector256<float>> xSpan = MemoryMarshal.Cast<float, Vector256<float>>(xPoints.Span);
            Span<Vector256<float>> res = MemoryMarshal.Cast<float, Vector256<float>>(results2.Span);
            Span<Vector256<float>> testSpan = MemoryMarshal.Cast<float, Vector256<float>>(testValue2.Span);            
            int resVectorNumber = 0;

            Vector256<float> xVec, yVec;
            var oneVec = Vector256.Create(1.0f);
            var fourVec = Vector256.Create(4.0f);

            while (countY < ySpan.Length)
            {
                var currYVec = Vector256.Create(ySpan[countY]);
                while (countX < xSpan.Length)
                {
                    
                    Vector256<float> currXVec = xSpan[countX];
                    var xSquVec = Vector256.Create(0.0f);
                    var ySquVec = Vector256.Create(0.0f);
                    var zSquVec = Vector256.Create(0.0f);
                    var interVec = Vector256.Create(0.0f);
                    Vector256<float> sumVector = oneVec;
                    inter = 0;
                    bool goOn = true;
                    while (goOn)
                    {
                        xVec = Avx.Add(Avx.Subtract(xSquVec, ySquVec), currXVec);
                        yVec = Avx.Add(Avx.Subtract(Avx.Subtract(zSquVec, ySquVec), xSquVec), currYVec);
                        xSquVec = Avx.Multiply(xVec, xVec);
                        ySquVec = Avx.Multiply(yVec, yVec);
                        zSquVec = Avx.Multiply(Avx.Add(xVec, yVec), Avx.Add(xVec, yVec));
                        Vector256<float> test = Avx.Compare(Avx.Add(xSquVec, ySquVec), fourVec, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling); // <= 4.0?
                        sumVector = Avx.BlendVariable(Vector256<float>.Zero, sumVector, test); // selects from second if true, from first otherwise                        
                        goOn = (Avx.MoveMask(test) > 0) & (inter < maxInter); //any of the values still alive, and inter still below cutoff value? 
                        if (goOn)
                        {
                            interVec = Avx.Add(interVec, sumVector);
                        }
                        inter = goOn ? inter + 1 : inter;
                    }
                    testSpan[resVectorNumber] = Avx.Add(xSquVec, ySquVec);                    
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
