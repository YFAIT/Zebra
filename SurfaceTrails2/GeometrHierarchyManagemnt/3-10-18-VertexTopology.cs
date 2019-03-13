using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Composite;

namespace SurfaceTrails2.BrepOperations
{
    public class _3_10_18_EdgeTopology : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _3_10_18_EdgeTopology class.
        /// </summary>
        public _3_10_18_EdgeTopology()
          : base("vertexTopology", "Topology",
              "Analyses vertex topology of any given edge network",
              "YFAtools", "GeometrHierarchyManagemnt")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Edges", "E", "Edges to analyse", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Valence", "V", "The desired number of connection per vertex", GH_ParamAccess.item,2);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("pt", "pt", "Points of the desired valence", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var allCurvesEdges = new List<Curve>();
            int valence = 0;
            var allBrepOuterPoints = new List<Point3d>();
            var dupPoints = new List<Point3d>();
            var dupPointCount = new List<int>();
            var pointsOfValence = new List<Point3d>();
            //get variables from grasshopper
            if (!DA.GetDataList(0, allCurvesEdges)) return;
            if (!DA.GetData(1,ref valence)) return;
            //get all brep points in a list
            foreach (Curve allCurvesEdge in allCurvesEdges)
            {
                allBrepOuterPoints.Add(allCurvesEdge.PointAtStart);
                allBrepOuterPoints.Add(allCurvesEdge.PointAtEnd);
            }
            //compare each point in the list of all point to new list of a duplicate from the all point list
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
            //putting resuls of comparison in their corresponding lists
            for (int i = 0; i < dupPoints.Count; i++)
            {
                if(dupPointCount[i] ==valence)
                    pointsOfValence.Add(dupPoints[i]);
            }
            //Export data to grasshopper
            var a = pointsOfValence;
            DA.SetDataList(0, a);
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
            get { return new Guid("d769c9eb-58c2-4597-9edf-a0c029ae773c"); }
        }
    }
}