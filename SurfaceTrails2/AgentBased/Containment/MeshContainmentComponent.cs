using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

//This Component control containment in a Closed Mesh boundary

namespace SurfaceTrails2.AgentBased.Containment
{
    public class MeshContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshContainmentComponent class.
        /// </summary>
        public MeshContainmentComponent()
          : base("MeshContainment", "Nickname",
              "Description",
              "Zebra",
              "AgentBased")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MeshContainer", "C", "MeshContainer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            MeshContainment container = new MeshContainment();
            Mesh mesh= new Mesh();
            double multiplier = 1.0;

            DA.GetData("Mesh", ref mesh);
            DA.GetData("Multiplier", ref multiplier);


            container.Mesh = mesh;
            container.Multiplier = multiplier;


            DA.SetData("MeshContainer", container);
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
                return Resources.MeshContainer;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("78870a08-dfe9-419b-83ef-cdb678ab929c"); }
        }
    }
}