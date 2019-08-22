using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This component controls the attracting behaviour for the the flock
namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class AttractingBehaviourComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AttractingBehaviourComponent class.
        /// </summary>
        public AttractingBehaviourComponent()
          : base("AttractingBehaviour", "Attract",
              "Initiates attractions toward a point or points",
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
            pManager.AddCircleParameter("Circles", "C", "Circles as attractor", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Attract to Nearest Point", "N", "attracts agents to nearest point to their start point", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Multiplier", "M", "strength of the behaviour", GH_ParamAccess.item, 1);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AttractorBehaviour", "B", "Attractor Behaviour to supply to container input in flocking engine", GH_ParamAccess.item);
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
            Attractor attractor = new Attractor();
            List<Circle> circles = new List<Circle>();
            double multiplier = 1.0;
            bool attractToNearestPt = true;
            //get values from grasshopper
            DA.GetDataList("Circles", circles);
            DA.GetData(1 , ref attractToNearestPt);
            DA.GetData("Multiplier", ref multiplier);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            attractor.Circles = circles;
            attractor.Multiplier = multiplier;
            attractor.AttractToNearestPt = attractToNearestPt;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("AttractorBehaviour", attractor);
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
                return Resources.AttractionBehaviour;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("49749d6a-63eb-4a10-95f6-8b0e435e3ee9"); }
        }
    }
}