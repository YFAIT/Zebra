using System;
using Rhino.Geometry;
//This Class contains core methods for any Vector operation used in my code
namespace SurfaceTrails2.OperationLibrary
{
    static class VectorOperations
    {
        static Random random = new Random();
        // ===============================================================================================
        // gets a random vector in which the values are between 0.00 and 1.00 in X,Y and Z
        // ===============================================================================================
        public static Vector3d GetRandomUnitVector()
        {
            double phi = 2.0 * Math.PI * random.NextDouble();
            double theta = Math.Acos(2.0 * random.NextDouble() - 1.0);
            //Mathimatical Operations to get values between 0 and 1 using sin and cos
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);
            //Return values to method
            return new Vector3d(x, y, z);
        }
        // ===============================================================================================
        // gets a random vector in which the values are between 0.00 and 1.00 in X,Y (2d plane)
        // ===============================================================================================
        public static Vector3d GetRandomUnitVectorXY()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();
            //Mathimatical Operations to get values between 0 and 1 using sin and cos
            double x = Math.Cos(angle);
            double y = Math.Sin(angle);
            //Return values to method
            return new Vector3d(x, y, 0.0);
        }
        // ===============================================================================================
        // gets a random vector in which the values are between 0.00 and 1.00 in X only
        // ===============================================================================================
        public static double GetRandomUnitVectorX()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();
            //Mathimatical Operations to get values between 0 and 1 using s cos
            double x = Math.Cos(angle);
            //Return values to method
            return x;
        }
        // ===============================================================================================
        // gets a random vector in which the values are between 0.00 and 1.00 in Y only
        // ===============================================================================================
        public static double GetRandomUnitVectorY()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();
            //Mathimatical Operations to get values between 0 and 1 using sin 
            double y = Math.Sin(angle);
            //Return values to method
            return y;
        }
    }
}
