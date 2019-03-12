using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace SurfaceTrails2.Composite
{
    public static class NailPlacer
    {
        public static List<Point3d> Nail(Curve curve, double distance)
        {
            List<Point3d> points = new List<Point3d>();

            points.Add(curve.PointAtLength((curve.GetLength()/2) + (distance / 2)));
            points.Add (curve.PointAtLength((curve.GetLength() / 2) + (-distance / 2)));

            return points;
        }
    }
}
