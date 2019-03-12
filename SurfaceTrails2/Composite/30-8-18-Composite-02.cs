﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
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
            //pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Center Mark", "Center", "Center Mark scale", GH_ParamAccess.item);
            pManager.AddNumberParameter("Composite thickness", "Thickness", "Thickness of YFA composite", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Conintuity", "c", "Continuity", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddCurveParameter("Center Mark", "Center","Centermark of the composite cells",GH_ParamAccess.item);
            //pManager.AddCurveParameter("Nails Locations","Nails","Location of nails on YFA composite",GH_ParamAccess.item);
            //pManager.AddCurveParameter("Weaving Paths", "curves", "Weaving paths of the YFA composite", GH_ParamAccess.item);
            //pManager.AddSurfaceParameter("srf", "srf", "srf", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "crv", "crv", GH_ParamAccess.item);
            pManager.AddPointParameter("point", "pt", "pt", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //List<Brep> breps = new List<Brep>();
            Brep brep = null;
            double nakedLength = 5;
            double clothedWidth = 1;
            //double centerMark = 1;
            double thickness = 2;
            if (!DA.GetData(0, ref brep)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            //if (!DA.GetData(3, ref centerMark)) return;
            if (!DA.GetData(3, ref thickness)) return;
            if (!DA.GetData(4, ref continuity)) return;

            var discontinuities = new List<double>();
            var extendedEdges = new List<Curve>();
            var intersectionPoints = new List<Point3d>();

            var nakedEdges = brep.DuplicateNakedEdgeCurves(true, false);
            Curve[] border = Curve.JoinCurves(nakedEdges);
            var borderCurve = border[0] ;
            var borderPoints = new List<Point3d>();
            List<double> distance = new List<double>();

            var t0 = borderCurve.Domain.T0;
            var t1 = borderCurve.Domain.T1;

            discontinuities.Add(0);
            while (borderCurve.GetNextDiscontinuity((Continuity) continuity, t0, t1, out t0))
                discontinuities.Add(t0);

            foreach (double discontinuity in discontinuities)
                borderPoints.Add(borderCurve.PointAt(discontinuity));

            var explodedEdges = borderCurve.DuplicateSegments();
            

            foreach (var explodedEdge in explodedEdges)
            {
                var offset = explodedEdge.Offset(Plane.WorldXY, thickness, DocumentTolerance(), CurveOffsetCornerStyle.Sharp);
                extendedEdges.Add(offset[0].Extend(CurveEnd.Both,0.1,CurveExtensionStyle.Line)); 
            }

            int a = extendedEdges.Count();
            for (int i = 0; i < (a); i++)
            {
                var shiftExtended = ListOperations.Shift(extendedEdges, 1);
                CurveIntersections  intersection =   Intersection.CurveCurve(extendedEdges[i], shiftExtended[i], DocumentTolerance(), DocumentTolerance());
                    foreach (var pointerSection in intersection)
                       intersectionPoints.Add(pointerSection.PointA);
            }
            Polyline outputPolyline = new Polyline(intersectionPoints);
            var pointCloser = new List<Point3d> {intersectionPoints[0], intersectionPoints[(intersectionPoints.Count-1)]};
            Polyline polylineCloser = new Polyline(pointCloser);
            Curve outputCurve = outputPolyline.ToNurbsCurve();
            Curve CurveCloser = polylineCloser.ToNurbsCurve();
            Curve[] summedPolyline = { CurveCloser, outputCurve };
            var closedPolylineArray = Curve.JoinCurves(summedPolyline);
            var closedCurve = closedPolylineArray[0];
            closedCurve.DuplicateSegments();

            //var a = faces;
            var b = closedCurve;
            var c = intersectionPoints;
            
                //DA.SetDataTree(0, a);
                DA.SetData(0, b);
                DA.SetDataList(1, c);
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