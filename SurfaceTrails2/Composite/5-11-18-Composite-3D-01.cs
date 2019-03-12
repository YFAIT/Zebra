using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace SurfaceTrails2.Composite
{
    public class _1_11_18_Composite_3D_01 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _1_11_18_Composite_3D_01 class.
        /// </summary>
        public _1_11_18_Composite_3D_01()
          : base("Composit 3D", "Composite3D",
              "Creates YFA composite lines with respect to it's thickness and 3d Boundary",
              "YFAtools", "Composite")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "b", "Brep to make the YFA composite system on", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item, 0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Composite points", "pt", "pt", GH_ParamAccess.tree);
            pManager.HideParameter(0);
            pManager.AddCurveParameter("crv", "Composite curve", "crv", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "Brep Curve", "crv", GH_ParamAccess.list);
            pManager.HideParameter(2);
            pManager.AddPointParameter("Centermarks", "pt", "pt", GH_ParamAccess.tree);
            pManager.HideParameter(3);
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

            if (!DA.GetDataList(0, breps)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;

            var sortedCompositePoints = new DataTree<Point3d>();
            DataTree<Curve> sortedCompositeCurves = new DataTree<Curve>();
            DataTree<Curve> borderTree = new DataTree<Curve>();

            DataTree<Point3d> closestPointTree = new DataTree<Point3d>();
            DataTree<Point3d> compositePoints = new DataTree<Point3d>();

            //Getting Naked edges 
            var allCurvesEdgesTree = new DataTree<Curve>();

            var allCurvesEdgesSortedTree = new DataTree<Curve>();
            var allCurvesEdgesSortedMidTree = new DataTree<Point3d>();
            var allCurvesEdgesSortedboolTree = new DataTree<bool>();

            int b = 0;
            foreach (Brep brep in breps)
            {
                var nakedEdges = brep.DuplicateEdgeCurves(true);
                var nakedEdgeMidList = new List<Point3d>();
                var nakedEdgeboolList = new List<bool>();
                var interiorEdgeMidList = new List<Point3d>();
                var interiorEdgeboolList = new List<bool>();
                var allCurvesEdges = brep.DuplicateEdgeCurves(false);
                //getting interior edges
                var interiorEdges = new List<Curve>();
                var allEdges = brep.Edges;

                int l = 0;
                foreach (BrepEdge brepEdge in allEdges)
                {
                    if (brepEdge.Valence == EdgeAdjacency.Interior)
                    {
                        interiorEdges.Add(allCurvesEdges[l]);
                    }
                    l++;
                }
                foreach (Curve nakedEdge in nakedEdges)
                {
                    nakedEdgeMidList.Add(nakedEdge.PointAtNormalizedLength(0.5));
                    nakedEdgeboolList.Add(true);
                }
                foreach (Curve interiorEdge in interiorEdges)
                {
                    interiorEdgeMidList.Add(interiorEdge.PointAtNormalizedLength(0.5));
                    interiorEdgeboolList.Add(false);
                }
                allCurvesEdgesTree.AddRange(allCurvesEdges, new GH_Path(b));

                allCurvesEdgesSortedTree.AddRange(nakedEdges, new GH_Path(b));
                allCurvesEdgesSortedMidTree.AddRange(nakedEdgeMidList, new GH_Path(b));
                allCurvesEdgesSortedboolTree.AddRange(nakedEdgeboolList, new GH_Path(b));

                allCurvesEdgesSortedTree.AddRange(interiorEdges, new GH_Path(b));
                allCurvesEdgesSortedMidTree.AddRange(interiorEdgeMidList, new GH_Path(b));
                allCurvesEdgesSortedboolTree.AddRange(interiorEdgeboolList, new GH_Path(b));
                //deconstruct brep to get faces, get centroids and get curves to sort along
                List<Curve> curvesToSortAlong = new List<Curve>();
                var centroids = new List<Point3d>();

                    for(int i=0;i< brep.Faces.Count;i++)
                    {
                       
                        var ptx = new List<double>();
                        var pty = new List<double>();
                        var ptz = new List<double>();
                        var faceVertices = brep.Faces[i].DuplicateFace(false).DuplicateVertices();
                        var faceEdges = brep.Faces[i].DuplicateFace(false).DuplicateEdgeCurves();
                    
                        foreach (Point3d vertex in faceVertices)
                        {
                            ptx.Add(vertex.X);
                            pty.Add(vertex.Y);
                            ptz.Add(vertex.Z);
                        }
                        var ptAverage = new Point3d(ptx.Average(), pty.Average(), ptz.Average());
                        centroids.Add(ptAverage);

                        var faceBoundary = Curve.JoinCurves(faceEdges);
                        curvesToSortAlong.Add(faceBoundary[0]);
                    }
                //4 closest points to each centoid & Composite Points
                var indices = new List<int>();

                    for (int m = 0; m < centroids.Count; m++)
                    {
                        var closestPoints = PointOperations.ClosestPointsWithIndex( centroids[m], allCurvesEdgesSortedMidTree.Branch(b), 4, out indices);
                        closestPointTree.AddRange(closestPoints, new GH_Path(b, m));
                        for (int n = 0; n < closestPoints.Count; n++)
                        {
                            if (allCurvesEdgesSortedboolTree.Branch(b)[indices[n]])
                            {
                                compositePoints.Add(allCurvesEdgesSortedTree.Branch(b)[indices[n]].ToNurbsCurve().PointAtLength(allCurvesEdgesSortedTree.Branch(b)[indices[n]].GetLength() * 0.5 + nakedLength * 0.5), new GH_Path(b, m));
                                compositePoints.Add(allCurvesEdgesSortedTree.Branch(b)[indices[n]].ToNurbsCurve().PointAtLength(allCurvesEdgesSortedTree.Branch(b)[indices[n]].GetLength() * 0.5 - nakedLength * 0.5), new GH_Path(b, m));
                            }
                            else
                            {
                                compositePoints.Add(allCurvesEdgesSortedTree.Branch(b)[indices[n]].ToNurbsCurve().PointAtLength(allCurvesEdgesSortedTree.Branch(b)[indices[n]].GetLength() * 0.5 + clothedWidth * 0.5), new GH_Path(b, m));
                                compositePoints.Add(allCurvesEdgesSortedTree.Branch(b)[indices[n]].ToNurbsCurve().PointAtLength(allCurvesEdgesSortedTree.Branch(b)[indices[n]].GetLength() * 0.5 - clothedWidth * 0.5), new GH_Path(b, m));
                            }
                        }
                        var sortedPointList = PointOperations.SortAlongCurve(curvesToSortAlong[m], compositePoints.Branch(b, m));
                        for (int i = 0; i < sortedPointList.Count; i++)
                        {
                            sortedCompositePoints.Add(sortedPointList[i], new GH_Path(b, m));

                    }

                    var compositeCurve = CurveOperations.ClosedPolylineFromPoints(sortedCompositePoints.Branch(b, m));
                        sortedCompositeCurves.Add(compositeCurve, new GH_Path(b, m));
                    }
                
                var border = Curve.JoinCurves(allCurvesEdges);
                borderTree.Add(border[0], new GH_Path(b));
                b++;
            }
            var x = sortedCompositePoints;
            var y = sortedCompositeCurves;
            var z = borderTree;
            var u = closestPointTree;

            DA.SetDataTree(0, x);
            DA.SetDataTree(1, y);
            DA.SetDataTree(2, z);
            DA.SetDataTree(3, u);
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