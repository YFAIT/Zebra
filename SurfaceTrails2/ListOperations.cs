using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceTrails2
{
    public static class ListOperations
    {
        public static T[] Shift<T>(IList<T> input, int shift)   
        {
            int l = input.Count;

            if (shift < 0)
                shift = (shift % l) + l;

            T[] result = new T[l];
            for (int i = 0; i < l; i++)
            {
                result[i] = input[(shift + i) % l];
            }
            return result;
        }
    }
}
