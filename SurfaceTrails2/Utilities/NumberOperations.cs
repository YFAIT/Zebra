namespace SurfaceTrails2.Utilities
{
   public static  class NumberOperations
    {
      public  static double remap(double oldRangeT0, double oldRangeT1, double newRangeT0, double newRangeT1, double valueToRemap)
        {
            return newRangeT0 + (newRangeT1 - newRangeT0) * ((valueToRemap - oldRangeT0) / (oldRangeT1 - oldRangeT0));
        }

    }
}
