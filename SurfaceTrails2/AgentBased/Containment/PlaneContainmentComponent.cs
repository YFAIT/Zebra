using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.AgentBased.Containment
{
    public class PlaneContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlaneContainmentComponent class.
        /// </summary>
        public PlaneContainmentComponent()
          : base("PlaneContainment", "Nickname",
              "Description",
              "YFAtools",
              "AgentBased")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PlaneContainer", "C", "PlaneContainer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var container = new PlaneContainment();
            Curve curve = null;
            double multiplier = 1.0;

            DA.GetData("Curve", ref curve);
            DA.GetData("Multiplier", ref multiplier);

            container.Curve = curve;
            container.Multiplier = multiplier;

            DA.SetData(0, container);
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
                return Resources.PlaneContainer;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b59b00cb-992d-416d-96e7-64a853d87a8f"); }
        }
    }
}