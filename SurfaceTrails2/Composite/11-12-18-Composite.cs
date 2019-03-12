using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using SurfaceTrails2.Properties;
using SurfaceTrails2.Utilities;

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
            pManager.AddMeshParameter("mesh", "m", "Mesh to make the YFA composite system on", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Composite thickness", "Thickness", "Thickness of YFA composite", GH_ParamAccess.item, 0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Composite points", "pt", "pt", GH_ParamAccess.tree);
            pManager.HideParameter(0);
            pManager.AddCurveParameter("crv", "Composite curve", "crv", GH_ParamAccess.tree);
            pManager.AddTextParameter("Profiling", "Profiling", "Time for major operations", GH_ParamAccess.list);
            pManager.AddPointParameter("Composite points", "pt", "pt", GH_ParamAccess.tree);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Stopwatch topologyEdgesWatch = new Stopwatch();
            Stopwatch addToTreeWatch = new Stopwatch();
            Stopwatch dispatchPointsWatch = new Stopwatch();
            Stopwatch edgesFromPointsWatch = new Stopwatch();

            double nakedLength = 0.05;
            double clothedWidth = 0.01;
            double thickness = 0.05;
            var meshes = new List<Mesh>();
            var profiling = new List<string>();

            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            if (!DA.GetData(3, ref thickness)) return;

            var edgeTopology = new List<int>();
            var allEdges = new List<Curve>();
            var allEdgeMids = new List<Point3d>();

            var centroids = new DataTree<Point3d>();

            var pts = new DataTree<Point3d>();
            var joinedEdgesTree = new DataTree<Curve>();
            var finalEdgesTree = new DataTree<Curve>();

            var allEdgesTree = new DataTree<Curve>();
            var edgeTopologyTree = new DataTree<int>();

            int b = 0;
            foreach (var mesh in meshes)
            {
                topologyEdgesWatch.Start();
                for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                {
                    edgeTopology.Add(mesh.TopologyEdges.GetConnectedFaces(i).Length);
                    allEdges.Add(mesh.TopologyEdges.EdgeLine(i).ToNurbsCurve());
                    allEdgeMids.Add(mesh.TopologyEdges.EdgeLine(i).PointAt(0.5));
                }
                //Organize edges in tree
                //centroid
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    var centroid = mesh.Faces.GetFaceCenter(i);
                    centroids.Add(centroid, new GH_Path(b));
                }
                //organizing
                for (int i = 0; i < centroids.Branch(b).Count; i++)
                {
                    var indices = new List<int>();
                    PointOperations.ClosestPointsWithIndex(centroids.Branch(b)[i], allEdgeMids, 4, out indices);
                    for (int j = 0; j < indices.Count; j++)
                    {
                        allEdgesTree.Add(allEdges[indices[j]], new GH_Path(b, i));
                        edgeTopologyTree.Add(edgeTopology[indices[j]], new GH_Path(b, i));
                    }
                }
                topologyEdgesWatch.Stop();

                addToTreeWatch.Start();
                for (int i = 0; i < allEdgesTree.BranchCount; i++)
                {
                    for (int j = 0; j < allEdgesTree.Branch(i).Count; j++)
                    {
                        var nurbs = allEdgesTree.Branch(i)[j];
                        var integer = edgeTopologyTree.Branch(i)[j];

                        var average = new Point3d(allEdgesTree.Branch(i).Average(p => p.PointAt(0.5).X),
                          allEdgesTree.Branch(i).Average(p => p.PointAt(0.5).Y)
                          , allEdgesTree.Branch(i).Average(p => p.PointAt(0.5).Z));

                        Point3d pt1;
                        Point3d pt2;
                        if (integer != 2)
                        {
                            var offset1 = nurbs.Offset(Plane.WorldXY, thickness, 0.01, CurveOffsetCornerStyle.Sharp);
                            var offset2 = nurbs.Offset(Plane.WorldXY, thickness * -1, 0.01, CurveOffsetCornerStyle.Sharp);
                            var distance1 = offset1[0].PointAt(0.5).DistanceTo(average);
                            var distance2 = offset2[0].PointAt(0.5).DistanceTo(average);
                            //Minimum offset length
                            var offsetMin = distance1 < distance2 ? offset1[0] : offset2[0];

                            pt1 = offsetMin.PointAtLength(nurbs.GetLength() * 0.5 + nakedLength * 0.5);
                            pt2 = offsetMin.PointAtLength(nurbs.GetLength() * 0.5 - nakedLength * 0.5);
                            pts.Add(pt1, new GH_Path(b, i));
                            pts.Add(pt2, new GH_Path(b, i));
                        }
                        else
                        {
                            pt1 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 + clothedWidth * 0.5);
                            pt2 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 - clothedWidth * 0.5);
                            pts.Add(pt1, new GH_Path(b, i));
                            pts.Add(pt2, new GH_Path(b, i));
                        }
                    }
                }
                addToTreeWatch.Stop();
                dispatchPointsWatch.Start();

                for (int i = 0; i < allEdgesTree.BranchCount; i++)
                {
                    var curveList = allEdgesTree.Branch(i);
                    joinedEdgesTree.Add(Curve.JoinCurves(curveList)[0], new GH_Path(b, i));
                }
                for (int i = 0; i < joinedEdgesTree.BranchCount; i++)
                {
                    var curveList = new List<Curve>();
                    for (int j = 0; j < joinedEdgesTree.Branch(i).Count; j++)
                    {
                        curveList = joinedEdgesTree.Branch(i);

                        var ptList = new List<Point3d>();
                        ptList.AddRange(pts.Branch(i));

                        var sortedPoints = PointOperations.SortAlongCurve(curveList[0], ptList);
                        finalEdgesTree.Add(CurveOperations.ClosedPolylineFromPoints(sortedPoints), new GH_Path(b));

                        ptList.Clear();
                    }
                    curveList.Clear();
                }
                b++;
                dispatchPointsWatch.Stop();
            }
            profiling.Add("Topology: "+topologyEdgesWatch.ElapsedMilliseconds);
            profiling.Add("Add to tree : " + addToTreeWatch.ElapsedMilliseconds);
            profiling.Add("Dispatch points : " + dispatchPointsWatch.ElapsedMilliseconds);

            var  x = pts;
            var  y = finalEdgesTree;
            var z = profiling;

            DA.SetDataTree(0, x);
            DA.SetDataTree(1, y);
            DA.SetDataList(2, z);
            //DA.SetDataList(3, u);
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