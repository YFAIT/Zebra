using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This Component controls containment in a surface boundary
namespace SurfaceTrails2.AgentBased.Containment
{
    public class SurfaceContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceContainmentComponent class.
        /// </summary>
        public SurfaceContainmentComponent()
          : base("Surface Containment", "SurfaceContainment",
              "controls containment in a surface boundary",
              "Zebra",
              "AgentBased")
        {
        }
        //Controls Place of component on grasshopper menu
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface container in which the flock will kept", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Strength of parameter", GH_ParamAccess.item, 1);

        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SurfaceContainer", "C", "Surface Container class to supply to container input in flocking engine",
                GH_ParamAccess.item);
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
// ===============================================================================================
// Read input parameters
// ===============================================================================================
            SurfaceContainment container = new SurfaceContainment();
            Surface surface = null;
            double multiplier = 1.0;
            var xMin = 0;
            var xMax = 30;
            var yMin = 0;
            var yMax = 30;
            //get values from grasshopper
            DA.GetData("Surface", ref surface);
            DA.GetData("Multiplier", ref multiplier);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            container.xMin = xMin;
            container.xMax = xMax;
            container.yMin = yMin;
            container.yMax = yMax;

            container.Surface = surface.ToNurbsSurface();
            container.Multiplier = multiplier;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("SurfaceContainer", container);
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
                return Resources.SurfaceContainer;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("14334d8b-f7eb-489e-9aca-0d6f9181c484"); }
        }
    }
}