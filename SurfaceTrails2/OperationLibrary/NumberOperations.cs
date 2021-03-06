﻿namespace SurfaceTrails2.OperationLibrary
{
    //This Class contains core methods for any Number operation used in my code
    public static  class NumberOperations
    {
// ===============================================================================================
// Remap numbers from one range to another
// ===============================================================================================
        public static double remap(double oldRangeT0, double oldRangeT1, double newRangeT0, double newRangeT1, double valueToRemap)
        {
            return newRangeT0 + (newRangeT1 - newRangeT0) * ((valueToRemap - oldRangeT0) / (oldRangeT1 - oldRangeT0));
        }
    }
}
