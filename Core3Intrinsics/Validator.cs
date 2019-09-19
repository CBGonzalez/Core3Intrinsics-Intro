using System;
using System.Collections.Generic;
using System.Text;

namespace Core3Intrinsics
{
    public static class Validator
    {
        public static (bool, List<int>) CompareValues<T>(T[] left, T[] right) where T : struct
        {
            var differIndexes = new List<int>();            
            bool allEqual = true;
            if(left.Length != right.Length)
            {
                throw new ArgumentOutOfRangeException($"Arrays not of the same length: {nameof(left)} {nameof(right)}.");
            }
            for(int i = 0; i < left.Length; i++)
            {
                if(!EqualityComparer<T>.Default.Equals(left[i], right[i]))
                {
                    differIndexes.Add(i);
                    
                    allEqual &= false;
                }
            }

            return (allEqual, differIndexes);
        }

        public static (bool, List<int>, int) CompareValuesFloat(float[] left, float[] right)
        {
            var differIndexes = new List<int>();
            int maxDifference = 0;
            bool allEqual = true;
            if (left.Length != right.Length)
            {
                throw new ArgumentOutOfRangeException($"Arrays not of the same length: {nameof(left)} {nameof(right)}.");
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    differIndexes.Add(i);
                    if(Math.Abs(left[i] - right[i]) > maxDifference)
                    {
                        maxDifference = (int)Math.Abs(left[i] - right[i]);
                    }
                    allEqual &= false;
                }
            }

            return (allEqual, differIndexes, maxDifference);
        }

        public static (bool, List<int>, int) CompareValuesDouble(double[] left, double[] right)
        {
            var differIndexes = new List<int>();
            int maxDifference = 0;
            bool allEqual = true;
            if (left.Length != right.Length)
            {
                throw new ArgumentOutOfRangeException($"Arrays not of the same length: {nameof(left)} {nameof(right)}.");
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    differIndexes.Add(i);
                    if (Math.Abs(left[i] - right[i]) > maxDifference)
                    {
                        maxDifference = (int)Math.Abs(left[i] - right[i]);
                    }
                    allEqual &= false;
                }
            }

            return (allEqual, differIndexes, maxDifference);
        }
    }
}
