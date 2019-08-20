using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

//This Component control containment outside a boundary, Also used as a precise repeller

namespace SurfaceTrails2.AgentBased.Containment
{
    public class ContainOutsideMeshComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ContainOutsideMeshComponent class.
        /// </summary>
        public ContainOutsideMeshComponent()
          : base("ContainOutsideMesh", "Nickname",
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
            pManager.AddGenericParameter("OutsideMeshContainer", "C", "OutsideMeshContainer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ContainOutsideMesh container = new ContainOutsideMesh();
            Mesh mesh = new Mesh();
            double multiplier = 1.0;

            DA.GetData("Mesh", ref mesh);
            DA.GetData("Multiplier", ref multiplier);


            container.Mesh = mesh;
            container.Multiplier = multiplier;


            DA.SetData("OutsideMeshContainer", container);
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
                return Resources.OutsideMeshContainer;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a4db0e00-47e8-4b11-a970-921d773785b8"); }
        }
    }
}