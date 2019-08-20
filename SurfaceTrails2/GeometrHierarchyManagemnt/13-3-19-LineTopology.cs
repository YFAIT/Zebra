using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.OperationLibrary;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.GeometrHierarchyManagemnt
{
    public class LineTopology : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LineTopology class.
        /// </summary>
        public LineTopology()
          : base("LineTopology", "Topology",
              "Analyses vertex topology of any given edge network",
              "Zebra", "GeometrHierarchyManagemnt")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "l", "Lines for which you have to find the topolgy", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Topology", "t", "Line topology(connectivity)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var lines = new List<Line>();
            //get varialbles from grasshopper
            if (!DA.GetDataList(0, lines)) return;
            //Line topology
           var topology = CurveOperations.LineTopology(lines, DocumentTolerance());
            //export to grasshopper 
            DA.SetDataList(0, topology);
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
                return Resources.LineTopology;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cdb238c3-eca6-4f54-8947-d4d9e3281d20"); }
        }
    }
}