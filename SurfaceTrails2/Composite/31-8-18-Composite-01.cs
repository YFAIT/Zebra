using System;
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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "b", "Brep to make the YFA composite system on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Center Mark", "Center", "Center Mark scale", GH_ParamAccess.item);
            pManager.AddNumberParameter("Composite thickness", "Thickness", "Thickness of YFA composite", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Composite points", "pt", "pt", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "Composite curve", "crv", GH_ParamAccess.list);
            pManager.AddCurveParameter("Nail circles", "circles", "Nail Location", GH_ParamAccess.tree);
            pManager.AddPointParameter("Centermarks", "pt", "pt", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Centermark curves", "Centermark curves", "Center mark curves", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "Brep Curve", "crv", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //List<Brep> breps = new List<Brep>();
            Brep brep = null;
            double nakedLength = 0.05;
            double clothedWidth = 0.01;
            //double centerMark = 1;
            double thickness = 2;
            if (!DA.GetData(0, ref brep)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            //if (!DA.GetData(3, ref centerMark)) return;
            if (!DA.GetData(3, ref thickness)) return;

            var extendedEdges = new List<Curve>();
            var intersectionPoints = new List<Point3d>();
            var closedCurvePointsList = new List<Point3d>();
            var interiorEdges = new List<Curve>();
            var centroids = new List<Point3d>();
            DataTree<Point3d> closestPoint = new DataTree<Point3d>();
            List<Curve> curvesToSortAlong = new List<Curve>();
            var sortedPoints = new DataTree<Point3d>();
            List<Curve> compositeCurveTree = new List<Curve>();
            var nailCircles = new DataTree<Circle>();
            var centerMarkPoints = new DataTree<Point3d>();
            var compositeCurveIntersectionSegments = new List<Curve>();
            var centerMarkCurves = new DataTree<Curve>();


            var nakedEdges = brep.DuplicateEdgeCurves(true);
            var allCurvesEdges = brep.DuplicateEdgeCurves(false);

            //getting interior edges
            var allEdges = brep.Edges;
            int l = 0;
            foreach (BrepEdge brepEdge in allEdges)
            {
                if (brepEdge.Valence == EdgeAdjacency.Interior)
                    interiorEdges.Add(allCurvesEdges[l]);
                    l++;
            }


            Curve[] border = Curve.JoinCurves(nakedEdges);
            var borderCurve = border[0] ;
            var explodedEdges = borderCurve.DuplicateSegments();

            foreach (var explodedEdge in explodedEdges)
            {
                var offset = explodedEdge.Offset(Plane.WorldXY, thickness, DocumentTolerance(), CurveOffsetCornerStyle.Sharp);
                extendedEdges.Add(offset[0].Extend(CurveEnd.Both,0.1,CurveExtensionStyle.Line)); 
            }

            for (int i = 0; i < extendedEdges.Count(); i++)
            {
                var shiftExtended = ListOperations.Shift(extendedEdges, 1);
                CurveIntersections  intersection =   Intersection.CurveCurve(extendedEdges[i], shiftExtended[i], DocumentTolerance(), DocumentTolerance());
                    foreach (var pointerSection in intersection)
                       intersectionPoints.Add(pointerSection.PointA);
            }

           var closedCurve = CurveOperations.ClosedPolylineFromPoints(intersectionPoints);
           var closedCurveSegements = closedCurve.DuplicateSegments();

            foreach (Curve closedCurveSegement in closedCurveSegements)
            {
              var closedCurvePoints =  NailPlacer.Nail(closedCurveSegement, nakedLength);
                closedCurvePointsList.AddRange(closedCurvePoints);
            }
            foreach (Curve interiorEdge in interiorEdges)
            {
                var closedCurvePoints = NailPlacer.Nail(interiorEdge, clothedWidth);
                closedCurvePointsList.AddRange(closedCurvePoints);
            }
            //deconstruct brep to get faces, get centroids and get curves to sort along
            foreach (BrepFace face in brep.Faces)
            {
                 var faceBrep = face.DuplicateFace(true);
                AreaMassProperties vmp = AreaMassProperties.Compute(faceBrep);
                Point3d pt = vmp.Centroid;
                centroids.Add(pt);

             var faceBoundary =   Curve.JoinCurves(faceBrep.DuplicateEdgeCurves());
                curvesToSortAlong.Add(faceBoundary[0]);
            }
            //8 closest points to each centoid
            int m = 0;
            foreach (Point3d centroid in centroids)
            {
                GH_Path path = new GH_Path(m);
             var pointOperation =   PointOperations.ClosestPoints(centroid, closedCurvePointsList);
                for(int n=0;n<pointOperation.Count;n++)
                closestPoint.Add(pointOperation[n],path);
                m++;
            }
            //Sorting points along curves in a new datatree
                for(int o=0;o<curvesToSortAlong.Count;o++)
                {
               
                 var branchToAdd =  PointOperations.SortAlongCurve(curvesToSortAlong[o], closestPoint.Branch(o));
                    GH_Path path = new GH_Path(o);
                    sortedPoints.AddRange(branchToAdd,path);
                }
            //Closed polyline to draw
            for (int q = 0; q < sortedPoints.BranchCount; q++)
            {
                var compositeCurve =CurveOperations.ClosedPolylineFromPoints(sortedPoints.Branch(q));

             compositeCurveTree.Add(compositeCurve);
            }
            //Drawing nails
            for (int r = 0; r < sortedPoints.BranchCount; r++)
            {
                for (int s = 0; s < 8; s++)
                {
                    Circle circle = new Circle(sortedPoints.Branch(r)[s], 0.01);
                    GH_Path path = new GH_Path(r);
                    nailCircles.Add(circle, path);
                }
            }
            //Edge centermark points
            int u = 0;
            foreach (Curve compositeCurve in compositeCurveTree)
            {
                var compositeCurveSegments = compositeCurve.DuplicateSegments();
                compositeCurveIntersectionSegments = CurveOperations.SortCurveByLength(compositeCurveSegments.ToList());

                for (int t = 0; t < compositeCurveIntersectionSegments.Count; t++)
                {
                    GH_Path path = new GH_Path(u);
                    var centerMarkPoint = compositeCurveIntersectionSegments[t].PointAtNormalizedLength(0.5);
                    centerMarkPoints.Add(centerMarkPoint,path);

                    var centerMarkPointx1 =new Point3d((centerMarkPoint.X + 0.005), centerMarkPoint.Y, centerMarkPoint.Z);
                    var centerMarkPointx2 =new Point3d((centerMarkPoint.X - 0.005), centerMarkPoint.Y, centerMarkPoint.Z);
                    var centerMarkPointy1 = new Point3d(centerMarkPoint.X, (centerMarkPoint.Y+0.005), centerMarkPoint.Z);
                    var centerMarkPointy2 = new Point3d(centerMarkPoint.X, (centerMarkPoint.Y - 0.005), centerMarkPoint.Z);
                    var centerMarkCurve1 =new Line(centerMarkPointx1, centerMarkPointx2);
                    var centerMarkCurve2 = new Line(centerMarkPointy1, centerMarkPointy2);
                    centerMarkCurves.Add(centerMarkCurve1.ToNurbsCurve(), path);
                    centerMarkCurves.Add(centerMarkCurve2.ToNurbsCurve(), path);
                    //var centerMarkCurve1 = Line()
                }
                u++;
            }
            //Centermark cross
            var c = sortedPoints;
            var d = compositeCurveTree;
            var e = nailCircles;
            var f = centerMarkPoints;
            var g = centerMarkCurves;
            var h = extendedEdges;

            DA.SetDataTree(0, c);
            DA.SetDataList(1, d);
            DA.SetDataTree(2, e);
            DA.SetDataTree(3, f);
            DA.SetDataTree(4, g);
            DA.SetDataList(5, h);
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