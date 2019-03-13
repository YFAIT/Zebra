using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using SurfaceTrails2.Composite;

namespace SurfaceTrails2
{
   public static class CurveOperations
    {
        public static Curve ClosedPolylineFromPoints(List<Point3d> PolylinePoints)
        {
            Polyline outputPolyline = new Polyline(PolylinePoints);
            var pointCloser = new List<Point3d> { PolylinePoints[0], PolylinePoints[(PolylinePoints.Count - 1)] };
            Polyline polylineCloser = new Polyline(pointCloser);

            var outputCurve = outputPolyline.ToNurbsCurve();
            var CurveCloser = polylineCloser.ToNurbsCurve();
            Curve[] summedPolyline = { CurveCloser, outputCurve };
            var closedPolylineArray = Curve.JoinCurves(summedPolyline);
            return closedPolylineArray[0];
        }

        public static List<Curve> SortCurveByLength(List<Curve> curves)
        {
            var length = new List<double>();
            var curvesArray = curves.ToArray();

            for (int i = 0; i < curves.Count; i++)
                length.Add(curves[i].GetLength());

            var lengthArray =  length.ToArray();

            Array.Sort(lengthArray,curvesArray);
           return curvesArray.ToList().GetRange(0, 4);
        }

        public static List<int> LineTopology(List<Line> lines, double tolerance)
        {
            var countTopoList = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                var topology = 0;
                for (int j = 0; j < lines.Count; j++)
                {
                    if (PointOperations.PointDifference(lines[i].From, lines[j].From) < tolerance
                        && PointOperations.PointDifference(lines[i].To, lines[j].To) < tolerance  ||
                        PointOperations.PointDifference(lines[i].From, lines[j].To) < tolerance
                        && PointOperations.PointDifference(lines[i].To, lines[j].From) < tolerance)
                        topology++;
                }
                countTopoList.Add(topology);
            }
            return countTopoList;
        }

    }
}
