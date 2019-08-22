using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This component controls the Repelling behaviour for the the flock
namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class RepellingBehaviourComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RepellingBehaviourComponent class.
        /// </summary>
        public RepellingBehaviourComponent()
          : base("Repelling Behaviour", "RepellingBehaviour",
              "controls the Repelling behaviour for the the flock",
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
            pManager.AddCircleParameter("Circles", "C", "Circles to repel agents", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "M", "strength of the behaviour", GH_ParamAccess.item, 1);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("repellerBehaviour", "B", "repeller Behaviour to supply to container input in flocking engine",
                GH_ParamAccess.list);
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
            Repeller repeller = new Repeller();
            List<Circle> circles = new List<Circle>();
            double multiplier = 1.0;
            //get values from grasshopper
            DA.GetDataList("Circles", circles);
            DA.GetData("Multiplier", ref multiplier);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            repeller.Multiplier = multiplier;
            repeller.Circles = circles;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("repellerBehaviour", repeller);
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
                return Resources.RepellerBehaviour;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8b65139b-4468-405d-924f-141168f5c787"); }
        }
    }
}