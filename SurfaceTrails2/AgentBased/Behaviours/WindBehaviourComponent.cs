using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

//This component controls the Wind behaviour for the the flock

namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class WindBehaviourComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WindBehaviourComponent class.
        /// </summary>
        public WindBehaviourComponent()
          : base("WindBehaviour", "Nickname",
              "Description",
              "Zebra",
              "AgentBased")
        {
        }
        //Controls Place of component on grasshopper menu
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("windBehaviour", "B", "windBehaviour", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Wind wind = new Wind();
            Vector3d vector = Vector3d.Unset;
            double multiplier = 1.0;
            DA.GetData("Vector",ref vector);
            DA.GetData("Multiplier", ref multiplier);

            wind.WindVec = vector;
            wind.Multiplier = multiplier;
            //wind.Label = 'w';

            //var info = "values are" + circles[0].Radius;
            DA.SetData("windBehaviour", wind);
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
                return Resources.WindBehaviour;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("82c53e37-102d-409f-babd-524437146d05"); }
        }
    }
}