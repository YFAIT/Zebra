﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.Composite
{
    public class Composite : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Composite class.
        /// </summary>
        public Composite()
          : base("Composite Lines", "Composite",
              "Creates YFA composite lines with respect to it's thickness",
              "YFAtools", "Composite")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "b", "Brep to make the YFA composite system on", GH_ParamAccess.list);
            pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item,0.05);
            pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item,0.01);
            pManager.AddNumberParameter("Composite thickness", "Thickness", "Thickness of YFA composite", GH_ParamAccess.item,0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Composite points", "pt", "pt", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "Composite curve", "crv", GH_ParamAccess.tree);
            pManager.AddPointParameter("Centermarks", "pt", "pt", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "Brep Curve", "crv", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var breps = new List<Brep>();
            double nakedLength = 0.05;
            double clothedWidth = 0.01;
            double thickness = 2;

            if (!DA.GetDataList(0,  breps)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            if (!DA.GetData(3, ref thickness)) return;

            var sortedPoints = new DataTree<Point3d>();
            var compositeCurveTree2 = new DataTree<Curve>();
            var centerMarkPoints = new DataTree<Point3d>();
            var OffsetMinList = new List<Curve>();
            var bordercurveList = new List<Curve>();

            //Getting Naked edges 
            var NakedEdgesTree = new DataTree<Curve>();
            var interiorEdgesTree = new DataTree<Curve>();
            var allCurvesEdgesTree = new DataTree<Curve>();

            int b = 0;
            foreach (Brep brep in breps)
            {
               var nakedEdges = brep.DuplicateEdgeCurves(true);
               var allCurvesEdges = brep.DuplicateEdgeCurves(false);
                
                //getting interior edges
                var interiorEdges = new List<Curve>();
                var allEdges = brep.Edges;

                int l = 0;
                foreach (BrepEdge brepEdge in allEdges)
                {
                    if (brepEdge.Valence == EdgeAdjacency.Interior)
                        interiorEdges.Add(allCurvesEdges[l]);
                    l++;
                }
                NakedEdgesTree.AddRange(nakedEdges,new GH_Path(b));
                interiorEdgesTree.AddRange(interiorEdges,new GH_Path(b));
                allCurvesEdgesTree.AddRange(allCurvesEdges,new GH_Path(b));

            //joinCurve
            Curve[] border = Curve.JoinCurves(nakedEdges);
            var borderCurve = border[0];
                bordercurveList.Add(borderCurve);

            var offset1 = borderCurve.Offset(Plane.WorldXY, thickness, DocumentTolerance(),
                CurveOffsetCornerStyle.Sharp);
            var offset2 = borderCurve.Offset(Plane.WorldXY, thickness * -1, DocumentTolerance(),
                CurveOffsetCornerStyle.Sharp);
            //Minimum offset length
            Curve offsetMin;
            if (offset1.Length < offset2.Length)
                offsetMin = offset1[0];

            else
                offsetMin = offset2[0];

                OffsetMinList.Add(offsetMin);

            //Interior curve & border intersection
            var intersectionPoints = new List<Point3d>();


            for (int i = 0; i < interiorEdges.Count; i++)
            {
                CurveIntersections intersection = Intersection.CurveCurve(interiorEdges[i], offsetMin,
                    DocumentTolerance(), DocumentTolerance());


                for (int j = 0; j < intersection.Count; j++)
                {
                    intersectionPoints.Add(intersection[j].PointA);
                }
            }

            // Valence from brep edges
            var allBrepOuterPoints = new List<Point3d>();
            var dupPoints = new List<Point3d>();
            var dupPointCount = new List<int>();

            foreach (Curve allCurvesEdge in allCurvesEdges)
            {
                allBrepOuterPoints.Add(allCurvesEdge.PointAtStart);
                allBrepOuterPoints.Add(allCurvesEdge.PointAtEnd);
            }

            foreach (Point3d brepPoint in allBrepOuterPoints)
            {
                bool exists = false;
                for (int i = 0; i < dupPoints.Count; i++)
                {
                    if (PointOperations.PointDifference(brepPoint, dupPoints[i]) < DocumentTolerance())
                    {
                        exists = true;
                        dupPointCount[i]++;
                    }
                }

                if (!exists)
                {
                    dupPointCount.Add(1);
                    dupPoints.Add(brepPoint);
                }
            }

            //getting brep corners
            var brepCorners = new List<Point3d>();
            for (int i = 0; i < dupPointCount.Count; i++)
            {
                if (dupPointCount[i] == 2)
                {
                    brepCorners.Add(dupPoints[i]);
                }
            }

            //projecting corners to offset curve
            var offsetMinCorners = new List<Point3d>();
            for (int i = 0; i < brepCorners.Count; i++)
            {
                double t;
                offsetMin.ClosestPoint(brepCorners[i], out t);
                offsetMinCorners.Add(offsetMin.PointAt(t));
            }

            //sorting all outer points along naked curve
            var allBrepPoints = new List<Point3d>();

            allBrepPoints.AddRange(offsetMinCorners);
            allBrepPoints.AddRange(intersectionPoints);
            //Outer brep lines
            var sortedBrepPoints = PointOperations.SortAlongCurve(borderCurve, allBrepPoints);
            var shiftedBrepPoints = ListOperations.Shift(sortedBrepPoints, 1);
            var outerLines = new List<Polyline>();
            var outerPoint1 = new List<Point3d>();
            var outerPoint2 = new List<Point3d>();
            //outer brep points
            for (int i = 0; i < sortedBrepPoints.Count; i++)
            {
                var outerLine = new Polyline() {sortedBrepPoints[i], shiftedBrepPoints[i]};
                outerPoint1.Add(outerLine.ToNurbsCurve().PointAtLength(outerLine.Length * 0.5 + nakedLength * 0.5));
                outerPoint2.Add(outerLine.ToNurbsCurve().PointAtLength(outerLine.Length * 0.5 - nakedLength * 0.5));
                outerLines.Add(outerLine);
            }

            //Inner segements & mid points
            var innerSegments = new List<Polyline>();
            var innerPoint1 = new List<Point3d>();
            var innerPoint2 = new List<Point3d>();

            for (int i = 0; i < interiorEdges.Count; i++)
            {
                var innerLine = new Polyline() {interiorEdges[i].PointAtStart, interiorEdges[i].PointAtEnd};
                innerPoint1.Add(innerLine.ToNurbsCurve().PointAtLength(innerLine.Length * 0.5 + clothedWidth * 0.5));
                innerPoint2.Add(innerLine.ToNurbsCurve().PointAtLength(innerLine.Length * 0.5 - clothedWidth * 0.5));
                innerSegments.Add(innerLine);
            }

            //all point to list
            var closedCurvePointsList = new List<Point3d>();

            closedCurvePointsList.AddRange(outerPoint1);
            closedCurvePointsList.AddRange(outerPoint2);
            closedCurvePointsList.AddRange(innerPoint1);
            closedCurvePointsList.AddRange(innerPoint2);
            //deconstruct brep to get faces, get centroids and get curves to sort along
            List<Curve> curvesToSortAlong = new List<Curve>();
            var centroids = new List<Point3d>();

            foreach (BrepFace face in brep.Faces)
            {
                var faceBrep = face.DuplicateFace(true);
                AreaMassProperties vmp = AreaMassProperties.Compute(faceBrep);
                Point3d pt = vmp.Centroid;
                centroids.Add(pt);

                var faceBoundary = Curve.JoinCurves(faceBrep.DuplicateEdgeCurves());
                curvesToSortAlong.Add(faceBoundary[0]);
            }

            //8 closest points to each centoid
            DataTree<Point3d> closestPoint = new DataTree<Point3d>();
            int m = 0;

            foreach (Point3d centroid in centroids)
            {
                GH_Path path = new GH_Path(m);
                var closestPoints = PointOperations.ClosestPoints(centroid, closedCurvePointsList, 8);
                for (int n = 0; n < closestPoints.Count; n++)
                    closestPoint.Add(closestPoints[n], path);
                m++;
            }

            //Sorting points along curves in a new datatree
            

            for (int o = 0; o < curvesToSortAlong.Count; o++)
                {
                    var branchToAdd = PointOperations.SortAlongCurve(curvesToSortAlong[o], closestPoint.Branch(o));

                    GH_Path path = new GH_Path(b,o);
                    sortedPoints.AddRange(branchToAdd, path);
                }
            //Closed polyline to draw
                var compositeCurveTree = new List<Curve>();

            for (int q = 0; q < closestPoint.BranchCount; q++)
                {
                    var compositeCurve = CurveOperations.ClosedPolylineFromPoints(sortedPoints.Branch(b,q));

                    compositeCurveTree.Add(compositeCurve);
                    compositeCurveTree2.Add(compositeCurve, new GH_Path(b,q));
                }
               
            var compositeCurveIntersectionSegments = new List<Curve>();
            int u = 0;

                foreach (Curve compositeCurve in compositeCurveTree)
                {
                    var compositeCurveSegments = compositeCurve.DuplicateSegments();
                    compositeCurveIntersectionSegments = CurveOperations.SortCurveByLength(compositeCurveSegments.ToList());

                    for (int t = 0; t < compositeCurveIntersectionSegments.Count; t++)
                    {
                        GH_Path path = new GH_Path(b,u);
                        var centerMarkPoint = compositeCurveIntersectionSegments[t].PointAtNormalizedLength(0.5);
                        centerMarkPoints.Add(centerMarkPoint, path);
                    }
                    u++;
                }
                b++;
            }
                var c = sortedPoints;
                var d = compositeCurveTree2;
                //var e = nailCircles;
                var f = centerMarkPoints;
                //var g = centerMarkCurves;
                var h = bordercurveList;

                DA.SetDataTree(0, c);
                DA.SetDataTree(1, d);
                //DA.SetDataTree(2, e);
                DA.SetDataTree(2, f);
                //DA.SetDataTree(4, g);
                DA.SetDataList(3, h);
        }
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources._30_8_18_CompositeLines;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3bd8113a-e9f6-4101-9d2b-e924d8d899c8"); }
        }
    }
}