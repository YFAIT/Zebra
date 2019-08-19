 using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
 using SurfaceTrails2.Properties;
 using SurfaceTrails2.Utilities;

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
            pManager.AddNumberParameter("Naked edge length", "l", "Length of edge on the naked sides of the brep", GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("Clothed edge width", "w", "width of edge on the clothed sides of the brep", GH_ParamAccess.item, 0.01);

        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Composite points", "p", "pt", GH_ParamAccess.tree);
            pManager.HideParameter(0);
            pManager.AddCurveParameter("crv", "c", "Composite curve", GH_ParamAccess.tree);
            pManager.AddTextParameter("Profiling", "P", "Time for major operations", GH_ParamAccess.list);
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var topologyEdgesWatch = new Stopwatch();
            var addToTreeWatch = new Stopwatch();
            var dispatchPointsWatch = new Stopwatch();
            var edgesFromPointsWatch = new Stopwatch();
            var meshes = new List<Mesh>();
            var nakedLength = 0.05;
            var clothedWidth = 0.01;
            var profiling = new List<string>();
            var joinedEdges = new List<Curve>();
            var segmentsList = new List<Line>();
            var segmentTreeFinal = new DataTree<Line>();
            var topoTreeFinal = new DataTree<int>();
            var ptTree = new DataTree<Point3d>();
            var ptTreeTemp = new DataTree<Point3d>();
            var compositeTree = new DataTree<Curve>();
            //get varialbles from grasshopper
            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            //applying code for each mesh in the mesh list
            var b = 0;
            foreach (var mesh in meshes)
            {
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    Point3f pta, ptb, ptc, ptd;
                    //get all 4 points from each mesh face in the mesh
                    mesh.Faces.GetFaceVertices(i, out pta, out ptb, out ptc, out ptd);
                    var pta0 = new Point3d(pta.X, pta.Y, pta.Z);
                    var pta1 = new Point3d(ptb.X, ptb.Y, ptb.Z);
                    var pta2 = new Point3d(ptc.X, ptc.Y, ptc.Z);
                    var pta3 = new Point3d(ptd.X, ptd.Y, ptd.Z);
                    var facePts = new List<Point3d> { pta0, pta1, pta2, pta3 };
                    //make closed Curve from all 4 points
                    var joinedCurves = CurveOperations.ClosedPolylineFromPoints(facePts);
                    joinedEdges.Add(joinedCurves);
                    facePts.Clear();
                    //convert curve to polyline
                    Polyline polyline;
                    if (!joinedEdges[i].TryGetPolyline(out polyline))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only polygonal curves are supported.");
                        return;
                    }
                    //make an unjoined list out of polyline segments
                    var segements = polyline.GetSegments();
                    segmentsList.AddRange(segements);
                }
                //find line topology
                var countTopoList = CurveOperations.LineTopology(segmentsList, DocumentTolerance());
                //make an organized data tree out of segements and their respective topology values
                var segmentTree = ListOperations.PartitionToTree<Line>(segmentsList, 4);
                var topoTree = ListOperations.PartitionToTree<int>(countTopoList, 4);

                for (int i = 0; i < segmentTree.BranchCount; i++)
                {
                    segmentTreeFinal.AddRange(segmentTree.Branch(i), new GH_Path(b, i));
                    topoTreeFinal.AddRange(topoTree.Branch(i), new GH_Path(b, i));
                }
                //make points out of organized edges 
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
                //make polyines from organized points
                for (int i = 0; i < ptTreeTemp.BranchCount; i++)
                {
                    var compositePolyline = CurveOperations.ClosedPolylineFromPoints(ptTreeTemp.Branch(i));
                    compositeTree.Add(compositePolyline, new GH_Path(b, i));
                }
                //clearing out all execessive data
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
                return Resources.Composite3d;
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