using System;
using Rhino.Geometry;

namespace SurfaceTrails2.Utilities
{
    static class Util
    {
        static Random random = new Random();

        public static Point3d GetRandomPoint(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            double x = minX + (maxX - minX) * random.NextDouble();
            double y = minY + (maxY - minY) * random.NextDouble();
            double z = minZ + (maxZ - minZ) * random.NextDouble();

            return new Point3d(x, y, z);
        }

        public static Vector3d GetRandomUnitVector()
        {
            double phi = 2.0 * Math.PI * random.NextDouble();
            double theta = Math.Acos(2.0 * random.NextDouble() - 1.0);

            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);

            return new Vector3d(x, y, z);
        }

        public static Vector3d GetRandomUnitVectorXY()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();

            double x = Math.Cos(angle);
            double y = Math.Sin(angle);

            return new Vector3d(x, y, 0.0);
        }
        public static double GetRandomUnitVectorX()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();

            double x = Math.Cos(angle);

            return x;
        }
        public static double GetRandomUnitVectorY()
        {
            double angle = 2.0 * Math.PI * random.NextDouble();

            double y = Math.Sin(angle);

            return y;
        }
    }
}
