using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This component controls the Follow Points behaviour for the the flock
namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class FollowOrganizedPointsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FollowOrganizedPointsComponent class.
        /// </summary>
        public FollowOrganizedPointsComponent()
          : base("Follow Organized Points Component", "FollowOrganizedPointsComponent",
              "controls the Follow Points behaviour for the the flock",
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
            pManager.AddCircleParameter("Circles", "C", "Sorted list of circles to follow by order", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "M", "strength of the behaviour", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Loop", "L", "makes agents go back to the start when they reach the last point of the list", GH_ParamAccess.item, true);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FollowPointsBehaviour", "B", "Follow Points Behaviour to supply to container input in flocking engine",
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
            List<Circle> circles = new List<Circle>();
            double multiplier = 1.0;
            bool loop = true;
            FollowOrganizedPoints follow = new FollowOrganizedPoints();
            //get values from grasshopper
            DA.GetDataList("Circles", circles);
            DA.GetData("Multiplier", ref multiplier);
            DA.GetData("Loop", ref loop);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            follow.Circles = circles;
            follow.Multiplier = multiplier;
            follow.Loop = loop;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("FollowPointsBehaviour", follow);
        }
        /// <summary>
        /// Provides an Icon for the component
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.FollowOrganisedPoints_01;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b851f1fb-e843-4016-b6e9-c236fce8dd45"); }
        }
    }
}