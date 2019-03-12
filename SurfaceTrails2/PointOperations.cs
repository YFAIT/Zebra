using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Rhino.Geometry.Intersect;



namespace SurfaceTrails2.Composite
{
    public static class PointOperations
    {

        public static List<Point3d> ClosestPoints(Point3d mainPoint, List<Point3d> closePoints)
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
            return orderedPointsList.GetRange(0, 8);
        }

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
