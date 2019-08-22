using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This Component controls containment in a brep boundary
namespace SurfaceTrails2.AgentBased.Containment
{
    public class BrepContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BrepContainmentComponent class.
        /// </summary>
        public BrepContainmentComponent()
          : base("Brep Containment", "BrepContainment",
              "controls containment in a brep boundary",
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
            pManager.AddBrepParameter("Brep", "B", "Brep container in which the flock will kept", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Strength of parameter", GH_ParamAccess.item, 1);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BrepContainer", "C", "Brep container class to supply to container input in flocking engine",
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
            BrepContainment container = new BrepContainment();
            Brep brep = null;
            double multiplier = 1.0;
            var weldedMesh = new Mesh();
            //get values from grasshopper
            DA.GetData("Brep", ref brep);
            DA.GetData("Multiplier", ref multiplier);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            var mesh = Mesh.CreateFromBrep(brep);
            for (int i = 0; i < mesh.Length; i++)
                weldedMesh.Append(mesh[i]);
            weldedMesh.Weld(0.01);

            container.Mesh = weldedMesh;
            container.Multiplier = multiplier;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("BrepContainer", container);
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
                return Resources.BrepContainer;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5454274c-2ef1-4642-8d24-5b15444583f5"); }
        }
    }
}