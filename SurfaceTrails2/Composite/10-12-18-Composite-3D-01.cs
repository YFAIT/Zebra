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
            pManager.AddCurveParameter("CurveTree", "CurveTree", "CurveTree", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("topology", "topology", "topology", GH_ParamAccess.tree);

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
            var edgeTopology = new List<GH_Integer>();
            var allEdges = new List<GH_Curve>();
            var pts = new GH_Structure<GH_Point>();
            var joinedEdgesTree = new/* GH_Structure<GH_Curve>();*/ DataTree<Curve>();
            var finalEdgesTree = new GH_Structure<GH_Curve>();
            var ptsTopoList = new List<Point3d> ();
            var countTopoList = new DataTree<int>();
            //get varialbles from grasshopper
            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            //getting all points from mesh and their topology
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
                //drawing composite point according to their valence
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
                //pts.Clear();
                //joining curves of component
                //for (int i = 0; i < allEdgesTree.PathCount; i++)
                //{

                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    //var GHCurveList= allEdgesTree.get_Branch(i);
                    //     var curveList = new List<Curve>();

                    //     foreach (GH_Curve ghCurve in GHCurveList)
                    //         curveList.Add(ghCurve.Value);

                    Point3f pta,ptb,ptc,ptd;

                        mesh.Faces.GetFaceVertices(i, out pta, out ptb, out ptc, out ptd);
                        var pta0 = new Point3d(pta.X, pta.Y, pta.Z);
                        var pta1 = new Point3d(ptb.X, ptb.Y, ptb.Z);
                        var pta2 = new Point3d(ptc.X, ptc.Y, ptc.Z);
                        var pta3 = new Point3d(ptd.X, ptd.Y, ptd.Z);
                        var facePts = new List<Point3d> {pta0, pta1, pta2, pta3};
                        ptsTopoList.Add(pta0);
                        ptsTopoList.Add(pta1);
                        ptsTopoList.Add(pta2);
                        ptsTopoList.Add(pta3);

                        var joinedCurves = CurveOperations.ClosedPolylineFromPoints(facePts);
                        joinedEdgesTree.Add(joinedCurves, new GH_Path(b, i));
                        facePts.Clear();

                    //joinedEdgesTree.Add(Curve.JoinCurves(curveList)[0], new GH_Path(b, i));
                }
                edgesFromPointsWatch.Start();
                //newcode
                for (int i = 0; i < joinedEdgesTree.BranchCount; i++)
                {
                    for (int j = 0; j < joinedEdgesTree.Branch(i).Count; j++)
                    {
                        //convert curve to polyline
                        Polyline polyline;
                        if (!joinedEdgesTree.Branch(i)[0].TryGetPolyline(out polyline))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only polygonal curves are supported.");
                            return;
                        }
                        //
                        Line[] segements = new Line[4];
                        segements = polyline.GetSegments();
                        for (int k =0; k< segements.Length; k++)
                        {
                            int topology = 0;
                            for (int c = 0; c < ptsTopoList.Count; c++)
                            {
                                var start = segements[k].From;
                                var end = segements[k].To;
                                var test = "test      ";

                                if (PointOperations.PointDifference(start, ptsTopoList[c]) < DocumentTolerance()
                                    || PointOperations.PointDifference(end, ptsTopoList[c]) < DocumentTolerance())
                                    topology++;

                            }
                            countTopoList.Add(topology ,new GH_Path(b, i));
                        }

                    }
                }
                //drawing final curves
                //for (int i = 0; i < joinedEdgesTree.BranchCount; i++)
                //{
                //    for (int j = 0; j < joinedEdgesTree.Branch(i).Count; j++)
                //    {
                //        //if (countTopoList.Branch(i)[j] = )
                //    }
                //}
                //sorting points and making a polyline out of them
                        //    for (int i = 0; i < joinedEdgesTree.BranchCount; i++)
                        //    {
                        //        for (int j = 0; j < joinedEdgesTree.Branch(i).Count; j++)
                        //        {
                        //            var joinedCurve = joinedEdgesTree.Branch(i)[0];

                        //            var GHptList = pts.get_Branch(i);
                        //            var ptList = new List<Point3d>();
                        //            foreach (GH_Point ghPt in GHptList)
                        //            {
                        //                ptList.Add(ghPt.Value);
                        //            }

                        //            var sortedPoints = PointOperations.SortAlongCurve(joinedCurve, ptList);

                        //            finalEdgesTree.Append(new GH_Curve(CurveOperations.ClosedPolylineFromPoints(sortedPoints)), new GH_Path(b));

                        //            //joinedCurve.Clear();
                        //            ptList.Clear();
                        //        }
                        //    }
                        //    edgesFromPointsWatch.Stop();
                        ptsTopoList.Clear();
                b++;
            }
            //profiling data preview in grasshopper
            profiling.Add("Topology: "+topologyEdgesWatch.ElapsedMilliseconds);
            profiling.Add("Add to tree: "+addToTreeWatch.ElapsedMilliseconds);
            profiling.Add("Dispatch points: "+dispatchPointsWatch.ElapsedMilliseconds);
            profiling.Add("Edges from points: "+edgesFromPointsWatch.ElapsedMilliseconds);
            //export data to grasshopper
            var x = pts;
            var y = finalEdgesTree;
            var z = profiling;
            var u = joinedEdgesTree;
            var v = countTopoList;
            DA.SetDataTree(0, x);
            DA.SetDataTree(1, y);
            DA.SetDataList(2, z);
            DA.SetDataTree(3, u);
            DA.SetDataTree(4, v);
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