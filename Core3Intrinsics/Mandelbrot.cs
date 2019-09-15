using System;
using System.Numerics;
using System.Runtime.Intrinsics;

using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Buffers;
using System.Threading.Tasks;

namespace Core3Intrinsics
{
    public class Mandelbrot
    {

        int TOTALBYTES = 16 * 1024 * 1024;//4 * 1024 * 1024;
        public int numberOfTasks = 1;

        const float LEFT_X = -2.5f;
        const float RIGHT_X = 1.0f;
        const float TOP_Y = 1.0f;
        const float BOTT_Y = -1.0f;
        
        int resolutionX, resolutionY;        
        float ratioy_x = (TOP_Y - BOTT_Y) / (RIGHT_X - LEFT_X);
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
            STEP_Y = ratioy_x * STEP_X;
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
                    bool goOn = true;
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
                    if (floatCounter == 13435)
                    {
                        string summy = string.Empty;
                    }
                }
                //currentX = LEFT_X;
                countX = 0;
                countY++;                               
            }
        }
        public void FloatMandelComplex()
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
            STEP_Y = ratioy_x * STEP_X;
            numberOfPoints = resolutionX * resolutionY;
            if (numberOfPoints % 8 != 0)
            {
                numberOfPoints += numberOfPoints % 8;
            }
            results2 = new float[numberOfPoints];
            testValue1 = new float[numberOfPoints];
            testValue2 = new float[numberOfPoints];

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
            Complex.FloatComplex currC, z;
            int countX = 0, countY = 0;
            int maxInter = 256;
            int inter;
            //float zSquare, xSquare, ySquare, x, y;
            ReadOnlySpan<float> ySpan = yPoints.Span;
            ReadOnlySpan<float> xSpan = xPoints.Span;
            Span<float> res = results2.Span;
            int floatCounter = 0;
            Complex.FloatComplex zeroComp = new Complex.FloatComplex(0, 0);
            while (countY < resolutionY)
            {
                currentY = ySpan[countY];
                while (countX < resolutionX)
                {
                    currentX = xSpan[countX];
                    //zSquare = xSquare = ySquare = 0.0f;
                    inter = 0;
                    currC = new Complex.FloatComplex(currentX, currentY);
                    z = zeroComp;
                    bool goOn = true;
                    while (goOn && inter < maxInter)
                    {
                        //x = xSquare - ySquare + currentX;
                        //y = zSquare - ySquare - xSquare + currentY;
                        //float tmpY = zSquare - ySquare - xSquare;
                        //Complex.FloatComplex tmp = Complex.Multiply(z, z);
                        //System.Diagnostics.Debug.Assert(tmpY == tmp.Y);
                        //Complex.FloatComplex tmp2 = Complex.Add(tmp, currC);
                        z = Complex.Add(Complex.Multiply(z, z), currC);
                        //System.Diagnostics.Debug.Assert(x == z.X && y == z.Y);
                        goOn = Complex.ModulusSquared(z) <= 4.0f;
                        //xSquare = x * x;
                        //ySquare = y * y;
                        //zSquare = (x + y) * (x + y);
                        //System.Diagnostics.Debug.Assert(xSquare + ySquare == Complex.ModulusSquared(z));
                        //goOn = xSquare + ySquare <= 4.0f;

                        inter = goOn ? inter + 1 : inter;
                    }
                    //res[countY * resolutionX + countX] = inter;
                    res[floatCounter] = inter;
                    //testValue2.Span[floatCounter] = xSquare + ySquare;
                    countX++;
                    floatCounter++;
                    if (floatCounter == 341)
                    {
                        string summy = string.Empty;
                    }
                }
                //currentX = LEFT_X;
                countX = 0;
                countY++;
            }
        }

        public void FloatMandelTask()
        {
            resolutionX = 1920;
            //if (resolutionX % sizeof(float) != 0)
            //{
            //    resolutionX -= resolutionX % sizeof(float);
            //}
            resolutionY = (int)MathF.Floor(resolutionX * ratioy_x);
            STEP_X = (RIGHT_X - LEFT_X) / resolutionX;
            STEP_Y = ratioy_x * STEP_X;
            numberOfPoints = resolutionX * resolutionY;
            //if (numberOfPoints % 8 != 0)
            //{
            //    numberOfPoints += numberOfPoints % 8;
            //}
            results = new float[numberOfPoints];

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
            int maxInter = 256;
            
            float zSquare, xSquare, ySquare, x, y;
            
            var tasks = new Task<bool>[numberOfTasks];
            for (int i = 0; i < numberOfTasks; i++)
            {
                int thisNumber = i;
                tasks[i] = Task.Run(() => OneTask(thisNumber, numberOfTasks));                
            }
            Task.WaitAll(tasks);
            return;

            bool OneTask(int whichTaskIsThis, int howManyTotalTasks)
            {
                float currentY = 0;
                float currentX = 0;
                int countX = 0, countY = 0;
                int inter;

                int defaultLinesPerTask = yPoints.Span.Length / howManyTotalTasks;
                int thisLineQuantity = defaultLinesPerTask;
                if (yPoints.Span.Length % howManyTotalTasks != 0 && whichTaskIsThis == howManyTotalTasks - 1)
                {
                    thisLineQuantity = yPoints.Span.Length - (howManyTotalTasks - 1) * defaultLinesPerTask; // add the missing line at the end
                }
                int yChunkStart = whichTaskIsThis * defaultLinesPerTask;
                ReadOnlySpan<float> ySpanChunk = yPoints.Span.Slice(yChunkStart, thisLineQuantity);

                ReadOnlySpan<float> xThisChunk = xPoints.Span;
                Span<float> resChunk = results.Span.Slice(yChunkStart * resolutionX, thisLineQuantity * resolutionX);
                int numberCounter = 0;
                
                while (countY < thisLineQuantity)
                {
                    currentY = ySpanChunk[countY];
                    while (countX < resolutionX)
                    {
                        currentX = xThisChunk[countX];
                        zSquare = xSquare = ySquare = 0.0f;
                        inter = 0;
                        bool goOn = true;
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

                        resChunk[numberCounter] = inter;
                        countX++;
                        numberCounter++;
                    }
                    countX = 0;
                    countY++;
                }
                return true;
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
            STEP_Y = ratioy_x * STEP_X;
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
            //int step = Vector256<float>.Count;
            int resVectorNumber = 0;//, xVectorNumber = 0;//, yVectorNumber = 0;

            Vector256<float> xVec, yVec;
            var oneVecInt = Vector256.Create(1);
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
                    //Avx.Store(rPtr, interVec);
                    res[resVectorNumber] = interVec;
                    resVectorNumber++;
                    if(resVectorNumber == 1867)
                    {
                        string dun = "blah";
                    }
                    //xVectorNumber++;
                    //countX += step;
                    countX++;
                    //xvecPtr += step;
                    //rPtr += step;
                }
                countX = 0;
                //xvecPtr = xSpPtr;
                //xVectorNumber = 0;
                countY++;
                //yvecPtr++;
                //yVectorNumber++;
            }

        }
    }
}
