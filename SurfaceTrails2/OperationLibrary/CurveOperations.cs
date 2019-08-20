using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace SurfaceTrails2.OperationLibrary
{
   public static class CurveOperations
    {
        //creates a closes curve out of a list of points
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
        //Sorts curves by the value of their lengths
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
        //Finds the connectivity of lines (duplicate line in mesh face boundaries so we know the line valency)
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
        //splits curve at kinks (discontinuity)
        public static bool CurveDiscontinuity(List<Curve> L, Curve crv, int continuity, bool recursive)
        {
            if (crv == null) { return false; }

            PolyCurve polycurve = crv as PolyCurve;
            if (polycurve != null)
            {
                if (recursive) { polycurve.RemoveNesting(); }

                Curve[] segments = polycurve.Explode();

                if (segments == null) { return false; }
                if (segments.Length == 0) { return false; }

                if (recursive)
                {
                    foreach (Curve S in segments)
                    {
                        return CurveDiscontinuity(L, S, continuity, recursive);
                    }
                }
                else
                {
                    foreach (Curve S in segments)
                    {
                        L.Add(S.DuplicateShallow() as Curve);
                    }
                }

                return true;
            }

            PolylineCurve polyline = crv as PolylineCurve;
            if (polyline != null)
            {
                if (recursive)
                {
                    for (int i = 0; i < (polyline.PointCount - 1); i++)
                    {
                        L.Add(new LineCurve(polyline.Point(i), polyline.Point(i + 1)));
                    }
                }
                else
                {
                    L.Add(polyline.DuplicateCurve());
                }
                return true;
            }

            Polyline p;
            if (crv.TryGetPolyline(out p))
            {
                if (recursive)
                {
                    for (int i = 0; i < (p.Count - 1); i++)
                    {
                        L.Add(new LineCurve(p[i], p[i + 1]));
                    }
                }
                else
                {
                    L.Add(new PolylineCurve());
                }
                return true;
            }

            //Maybe it’s a LineCurve?
            LineCurve line = crv as LineCurve;
            if (line != null)
            {
                L.Add(line.DuplicateCurve());
                return true;
            }

            //It might still be an ArcCurve…
            ArcCurve arc = crv as ArcCurve;
            if (arc != null)
            {
                L.Add(arc.DuplicateCurve());
                return true;
            }

            //Nothing else worked, lets assume it’s a nurbs curve and go from there…
            NurbsCurve nurbs = crv.ToNurbsCurve();
            if (nurbs == null) { return false; }

            double t0 = nurbs.Domain.Min;
            double t1 = nurbs.Domain.Max;
            double t;

            int LN = L.Count;

            do
            {
                if (!nurbs.GetNextDiscontinuity((Continuity)continuity, t0, t1, out t)) { break; }

                Interval trim = new Interval(t0, t);
                if (trim.Length < 1e-10)
                {
                    t0 = t;
                    continue;
                }

                Curve M = nurbs.DuplicateCurve();
                M = M.Trim(trim);
                if (M.IsValid) { L.Add(M); }

                t0 = t;
            } while (true);

            if (L.Count == LN) { L.Add(nurbs); }

            return true;
        }
    }
}
