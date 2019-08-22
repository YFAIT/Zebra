using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
//This Class contains core methods for any Point operation used in my code
namespace SurfaceTrails2.OperationLibrary
{
    public static class PointOperations
    {
        static Random random = new Random();
// ===============================================================================================
// gets a random point between specified bounds assigned by the user
// ===============================================================================================
        public static Point3d GetRandomPoint(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            double x = minX + (maxX - minX) * random.NextDouble();
            double y = minY + (maxY - minY) * random.NextDouble();
            double z = minZ + (maxZ - minZ) * random.NextDouble();
            return new Point3d(x, y, z);
        }
// ===============================================================================================
// gets one closest point to a certain point from a list of points and tells you its index in this list of points
// ===============================================================================================
        public static Point3d ClosestPointWithIndex(Point3d mainPoint, List<Point3d> closePoints, out int index)
        {
            double minDistance =0;
            index = 0;

            for (int i = 0; i < closePoints.Count; i++)
            {
                double distance = mainPoint.DistanceTo(closePoints[i]);

                if (i == 0)
                    minDistance = distance;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    index = i;
                }
             }
            return closePoints[index];
        }
        // ===============================================================================================
        //gets a chosen number closest points to a certain point from a list of points and tells you its indices of these points in this list of points
        // ===============================================================================================
        public static List<Point3d> ClosestPointsWithIndex( Point3d mainPoint, List<Point3d> closePoints, int numberOfPoints, out List<int> indices)
        {
            double[] distances = new double[closePoints.Count];
            Point3d[] orderedPoints = new Point3d[closePoints.Count];
            int[] indexList = new int[closePoints.Count];
            var distancesArray = new double[closePoints.Count];
            var distancesArray2 = new double[closePoints.Count];

                for (int i = 0; i < closePoints.Count; i++)
                {
                    double distance = mainPoint.DistanceTo(closePoints[i]);
                    distances[i] = distance;
                    indexList[i] = i;
                }
            //sorting
            distances.CopyTo(distancesArray, 0);
            distances.CopyTo(distancesArray2, 0);
            orderedPoints = closePoints.ToArray();

            Array.Sort(distancesArray, orderedPoints);
            var orderedPointsList = orderedPoints.ToList();

            var indicesToSort = indexList.ToArray();
            Array.Sort(distancesArray2, indicesToSort);
            indices = indicesToSort.ToList().GetRange(0, numberOfPoints);

            return orderedPointsList.GetRange(0, numberOfPoints);
        }
// ===============================================================================================
// gets a chosen number closest points to a certain point from a list of points
// ===============================================================================================
        public static List<Point3d> ClosestPoints(Point3d mainPoint, List<Point3d> closePoints, int numberOfPoints)
        {
            List<double> distances = new List<double>();
            Point3d[] orderedPoints = new Point3d[closePoints.Count];
            //double distance;

            foreach (Point3d closePoint in closePoints)
            {
               double  distance = mainPoint.DistanceTo(closePoint);
                distances.Add(distance);
            }

            orderedPoints = closePoints.ToArray();

            var distancesArray = distances.ToArray();

            Array.Sort(distancesArray ,orderedPoints );
           var orderedPointsList = orderedPoints.ToList();
            return orderedPointsList.GetRange(0, numberOfPoints);
        }
// ===============================================================================================
// Sorts points along a specified curve
// ===============================================================================================
        public static List<Point3d> SortAlongCurve(Curve curve, List<Point3d> points)
        {
            List<double> tParams = new List<double>();
            List<Point3d> sortedPoints = points;
            var sortedPointsArray = sortedPoints.ToArray();

            foreach (Point3d point in points)
            {
                double t;
                curve.ClosestPoint(point, out t);
                tParams.Add(t);
            }
            var tParamsArray = tParams.ToArray();

            Array.Sort(tParamsArray,sortedPointsArray);
           return sortedPointsArray.ToList();
        }
// ===============================================================================================
// substracts point coordintes from another so we can find duplicates
// ===============================================================================================
        public static double PointDifference(Point3d p1, Point3d p2)
        {
            return (Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) + Math.Abs(p1.Z - p2.Z));
        }
        //public static Point3d MovePointAlongcurve(Point3d pt, Curve curve)
        //{
        //    double t;
        //    curve.ClosestPoint(pt, out t);
        //    curve.PointAtLength()
        //}
    }
}
