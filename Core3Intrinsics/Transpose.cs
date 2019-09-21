using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core3Intrinsics
{
    public static class Transpose
    {
        private const int defWidth = 1920, defHeight = 1080, numberOfElements = 8;
        private static int currWidth, currHeight;
        private static int[] original;
        public static int[]transposed1, transposed2;

        private static bool isInitialized = false;

        public static bool SerializeColorsInt()
        {
            if(!isInitialized)
            {
                return false;
            }
            int[] colorComponents = new int[currWidth * 4];
            Span<int> colorsSpan = transposed1;
            int runningCounter = 0;//, byteCounter;            
            int start;
            for (int y = 0; y < currHeight; y++)
            {                               
                Span<int> currColors = colorsSpan.Slice(runningCounter, currWidth * 4);
                for (int x = 0; x < currWidth; x+= numberOfElements)
                {
                    for (int i = 0; i < numberOfElements; i++)
                    {                       
                        start = x * 4 + i;
                        colorComponents[start] = original[runningCounter];
                        colorComponents[start + numberOfElements] = original[runningCounter + 1];
                        colorComponents[start + (2 * numberOfElements)] = original[runningCounter + 2];
                        colorComponents[start + (3 * numberOfElements)] = original[runningCounter + 3];                        
                        runningCounter += 4;                        
                    }
                }
                colorComponents.CopyTo(currColors);
                
            }
            return true;
        }
        
        public static bool SerializedColorsVector256()
        {
            if (!isInitialized)
            {
                return false;
            }            
            Span<Vector256<int>> originVectors = MemoryMarshal.Cast<int, Vector256<int>>(original);
            Span<Vector256<int>> transposedVectors = MemoryMarshal.Cast<int, Vector256<int>>(transposed2);            
            Vector256<int> pm0, pm1, pm2, pm3, up0, up1, up2, up3;
            for(int i = 0; i < originVectors.Length; i += 4)
            {
                pm0 = Avx.Permute2x128(originVectors[i], originVectors[i + 2], 0x20);
                pm1 = Avx.Permute2x128(originVectors[i + 1], originVectors[i + 3], 0x20);
                pm2 = Avx.Permute2x128(originVectors[i], originVectors[i + 2], 0x31);
                pm3 = Avx.Permute2x128(originVectors[i + 1], originVectors[i + 3], 0x31);

                up0 = Avx2.UnpackLow(pm0, pm1);
                up1 = Avx2.UnpackHigh(pm0, pm1);
                up2 = Avx2.UnpackLow(pm2, pm3);
                up3 = Avx2.UnpackHigh(pm2, pm3);

                transposedVectors[i] = Avx2.UnpackLow(up0, up2);
                transposedVectors[i + 1] = Avx2.UnpackHigh(up0, up2);
                transposedVectors[i + 2] = Avx2.UnpackLow(up1, up3);
                transposedVectors[i + 3] = Avx2.UnpackHigh(up1, up3);
            }

            return true;
        }

        public static void CreateArrays(int width = defWidth, int height = defHeight)
        {
            currWidth = width;
            currHeight = height;

            original = new int[4 * currWidth * currHeight];
            transposed1 = new int[4 * currHeight * currWidth];
            transposed2 = new int[4 * currHeight * currWidth];

            for (int i = 0; i < original.Length; i++)
            {
                original[i] = i;
            }

            isInitialized = true;
        }
        
    }
}
