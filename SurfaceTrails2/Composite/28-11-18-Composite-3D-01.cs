using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SurfaceTrails2.Composite
{
    public class _28_11_18_Composite_3D_01 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _1_11_18_Composite_3D_01 class.
        /// </summary>
        public _28_11_18_Composite_3D_01()
          : base("Composit 3D", "Composite3D",
              "Creates YFA composite lines with respect to it's thickness and 3d Boundary",
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
            var meshes = new List<Mesh>();
            double nakedLength = 0.05;
            double clothedWidth = 0.01;
            var profiling = new List<string>();

            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;

            var edgeTopology = new List<GH_Integer>();
            var allEdges = new List<GH_Curve>();

            var pts = new GH_Structure<GH_Point>();
            var joinedEdgesTree = new GH_Structure<GH_Curve>();

            int b = 0;

            foreach (var mesh in meshes)
            {
                topologyEdgesWatch.Start();
                for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                {
                    edgeTopology.Add(new GH_Integer(mesh.TopologyEdges.GetConnectedFaces(i).Length));
                    allEdges.Add(new GH_Curve(mesh.TopologyEdges.EdgeLine(i).ToNurbsCurve()));
                }
                topologyEdgesWatch.Stop();

                addToTreeWatch.Start();
                var allEdgesTree = /*ListOperations.ReOrganize(*/ListOperations.PartitionToTree(allEdges, 3)/*)*/;
                var edgeTopologyTree = /*ListOperations.ReOrganize(*/ListOperations.PartitionToTree(edgeTopology, 3)/*)*/;
                addToTreeWatch.Stop();

                dispatchPointsWatch.Start();
                for (int i = 0; i < allEdgesTree.PathCount; i++)
                {
                    for (int j = 0; j < allEdgesTree.get_Branch(i).Count; j++)
                    {
                        var GHNurbs =(GH_Curve)allEdgesTree.get_Branch(i)[j];
                        var nurbs = GHNurbs.Value/*.ToNurbsCurve()*/;
                        var GHInteger = (GH_Integer) edgeTopologyTree.get_Branch(i)[j];

                        Point3d pt1;
                        Point3d pt2;
                        if (GHInteger.Value != 1)
                        {
                            pt1 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 + clothedWidth * 0.5);
                            pt2 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 - clothedWidth * 0.5);
                            pts.Append(new GH_Point(pt1), new GH_Path(b,i));
                            pts.Append(new GH_Point(pt2), new GH_Path(b,i));
                        }
                        else
                        {
                            pt1 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 + nakedLength * 0.5);
                            pt2 = nurbs.PointAtLength(nurbs.GetLength() * 0.5 - nakedLength * 0.5);
                            pts.Append(new GH_Point(pt1), new GH_Path(b, i));
                            pts.Append(new GH_Point(pt2), new GH_Path(b, i));
                        }
                    }
                }
                dispatchPointsWatch.Stop();

                allEdges.Clear();
                edgeTopology.Clear();

               
                for (int i = 0; i < allEdgesTree.PathCount; i++)
                {
               var GHCurveList= allEdgesTree.get_Branch(i);
                    var curveList = new List<Curve>();
                    foreach (GH_Curve ghCurve in GHCurveList)
                    {
                        curveList.Add(ghCurve.Value);
                    }
                     joinedEdgesTree.Append(new GH_Curve(Curve.JoinCurves(curveList)[0]), new GH_Path(b, i));
                 }
                edgesFromPointsWatch.Start();
                for (int i = 0; i < joinedEdgesTree.PathCount; i++)
                {
                    for (int j = 0; j < joinedEdgesTree.get_Branch(i).Count; j++)
                    {
                        var GHCurveList = joinedEdgesTree.get_Branch(i);
                        var curveList = new List<Curve>();
                        foreach (GH_Curve ghCurve in GHCurveList)
                        {
                            curveList.Add(ghCurve.Value);
                        }

                        var GHptList = pts.get_Branch(i);
                        var ptList = new List<Point3d>();
                        foreach (GH_Point ghPt in GHptList)
                        {
                            ptList.Add(ghPt.Value);
                        }


                        var sortedPoints = PointOperations.SortAlongCurve(curveList[0], ptList);
                            joinedEdgesTree.get_Branch(i)[j] =new GH_Curve(CurveOperations.ClosedPolylineFromPoints(sortedPoints)); ;
                    }
                }
                edgesFromPointsWatch.Stop();
                b++;
            }

            profiling.Add("Topology: "+topologyEdgesWatch.ElapsedMilliseconds);
            profiling.Add("Add to tree: "+addToTreeWatch.ElapsedMilliseconds);
            profiling.Add("Dispatch points: "+dispatchPointsWatch.ElapsedMilliseconds);
            profiling.Add("Edges from points: "+edgesFromPointsWatch.ElapsedMilliseconds);

            var x = pts;
            var y = joinedEdgesTree;
            var z = profiling;

            DA.SetDataTree(0, x);
            DA.SetDataTree(1, y);
            DA.SetDataList(2, z);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f55238f7-25c9-4468-92c3-7d78622584de"); }
        }
    }
}