using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.AgentBased.Containment
{
    public class BrepContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BrepContainmentComponent class.
        /// </summary>
        public BrepContainmentComponent()
          : base("BrepContainment", "Nickname",
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
            pManager.AddBrepParameter("Brep", "B", "Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BrepContainer", "C", "BrepContainer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BrepContainment container = new BrepContainment();
            Brep brep = null;
            double multiplier = 1.0;

            var weldedMesh = new Mesh();

            DA.GetData("Brep", ref brep);
            DA.GetData("Multiplier", ref multiplier);


            var mesh = Mesh.CreateFromBrep(brep);
            for (int i = 0; i < mesh.Length; i++)
                weldedMesh.Append(mesh[i]);
            weldedMesh.Weld(0.01);

            container.Mesh = weldedMesh;
            container.Multiplier = multiplier;


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