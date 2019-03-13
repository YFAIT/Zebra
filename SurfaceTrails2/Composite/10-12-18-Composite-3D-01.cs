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
            double nakedLength = 0.05;
            double clothedWidth = 0.01;
            var profiling = new List<string>();
            var edgeTopology = new List<GH_Integer>();
            var allEdges = new List<GH_Curve>();
            var pts = new GH_Structure<GH_Point>();
            var joinedEdges = new List<Curve>();
            var finalEdgesTree = new GH_Structure<GH_Curve>();
            var ptsTopoList = new List<Point3d>();
            var lineTopoList = new List<Line>();
            var countTopoList = new List<int>();
            var segmentsList = new List<Line>();
            var segmentTree = new DataTree<Line>();
            var topoTree = new DataTree<int>();
            var segmentTreeFinal = new DataTree<Line>();
            var topoTreeFinal = new DataTree<int>();
            var ptTree = new DataTree<Point3d>();
            var ptTreeTemp = new DataTree<Point3d>();
            var compositeTree = new DataTree<Curve>();
            //get varialbles from grasshopper
            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            //getting all points from mesh and their topology
            int b = 0;
            foreach (var mesh in meshes)
            {
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    Point3f pta, ptb, ptc, ptd;

                    mesh.Faces.GetFaceVertices(i, out pta, out ptb, out ptc, out ptd);
                    var pta0 = new Point3d(pta.X, pta.Y, pta.Z);
                    var pta1 = new Point3d(ptb.X, ptb.Y, ptb.Z);
                    var pta2 = new Point3d(ptc.X, ptc.Y, ptc.Z);
                    var pta3 = new Point3d(ptd.X, ptd.Y, ptd.Z);
                    var facePts = new List<Point3d> { pta0, pta1, pta2, pta3 };

                    var joinedCurves = ClosedPolylineFromPoints(facePts);
                    joinedEdges.Add(joinedCurves);
                    facePts.Clear();
                    //convert curve to polyline
                    Polyline polyline;
                    if (!joinedEdges[i].TryGetPolyline(out polyline))
                    {
                        //          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only polygonal curves are supported.");
                        return;
                    }

                    var segements = polyline.GetSegments();
                    segmentsList.AddRange(segements);

                }
                int topology;
                for (int k = 0; k < segmentsList.Count; k++)
                {
                    topology = 0;
                    for (int c = 0; c < segmentsList.Count; c++)
                    {
                        if (PointDifference(segmentsList[k].From, segmentsList[c].From) < RhinoDocument.ModelAbsoluteTolerance
                          && PointDifference(segmentsList[k].To, segmentsList[c].To) < RhinoDocument.ModelAbsoluteTolerance ||
                          PointDifference(segmentsList[k].From, segmentsList[c].To) < RhinoDocument.ModelAbsoluteTolerance
                          && PointDifference(segmentsList[k].To, segmentsList[c].From) < RhinoDocument.ModelAbsoluteTolerance)
                            topology++;
                    }
                    countTopoList.Add(topology);
                }
                segmentTree = PartitionToTree<Line>(segmentsList, 4);
                topoTree = PartitionToTree<int>(countTopoList, 4);

                for (int i = 0; i < segmentTree.BranchCount; i++)
                {
                    segmentTreeFinal.AddRange(segmentTree.Branch(i), new GH_Path(b, i));
                    topoTreeFinal.AddRange(topoTree.Branch(i), new GH_Path(b, i));
                }


                for (int i = 0; i < segmentTreeFinal.BranchCount; i++)
                {
                    for (int j = 0; j < segmentTreeFinal.Branch(i).Count; j++)
                    {
                        var segment = segmentTreeFinal.Branch(i)[j];
                        var topo = topoTreeFinal.Branch(i)[j];
                        Point3d pt1, pt2;

                        if (topo == 1)
                        {
                            pt1 = segment.ToNurbsCurve().PointAtLength(segment.ToNurbsCurve().GetLength() * 0.5 - nakedLength * 0.5);
                            pt2 = segment.ToNurbsCurve().PointAtLength(segment.ToNurbsCurve().GetLength() * 0.5 + nakedLength * 0.5);
                            ptTree.Add(pt1, new GH_Path(b, i));
                            ptTree.Add(pt2, new GH_Path(b, i));
                            ptTreeTemp.Add(pt1, new GH_Path(b, i));
                            ptTreeTemp.Add(pt2, new GH_Path(b, i));
                        }
                        else
                        {
                            pt1 = segment.ToNurbsCurve().PointAtLength(segment.ToNurbsCurve().GetLength() * 0.5 - clothedWidth * 0.5);
                            pt2 = segment.ToNurbsCurve().PointAtLength(segment.ToNurbsCurve().GetLength() * 0.5 + clothedWidth * 0.5);
                            ptTree.Add(pt1, new GH_Path(b, i));
                            ptTree.Add(pt2, new GH_Path(b, i));
                            ptTreeTemp.Add(pt1, new GH_Path(b, i));
                            ptTreeTemp.Add(pt2, new GH_Path(b, i));
                        }
                    }
                }
                for (int i = 0; i < ptTreeTemp.BranchCount; i++)
                {
                    var compositePolyline = ClosedPolylineFromPoints(ptTreeTemp.Branch(i));
                    compositeTree.Add(compositePolyline, new GH_Path(b, i));
                }
                ptTreeTemp.Clear();
                joinedEdges.Clear();
                segmentsList.Clear();
                countTopoList.Clear();
                segmentTree.Clear();
                topoTree.Clear();
                segmentTreeFinal.Clear();
                topoTreeFinal.Clear();
                b++;
            }
            //profiling data preview in grasshopper
            profiling.Add("Topology: "+topologyEdgesWatch.ElapsedMilliseconds);
            profiling.Add("Add to tree: "+addToTreeWatch.ElapsedMilliseconds);
            profiling.Add("Dispatch points: "+dispatchPointsWatch.ElapsedMilliseconds);
            profiling.Add("Edges from points: "+edgesFromPointsWatch.ElapsedMilliseconds);
            //export data to grasshopper
            var x = ptTree;
            var y = compositeTree;
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