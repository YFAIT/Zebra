using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public class FlockAgentComponent : GH_Component
    {
     
        /// <summary>
        /// Initializes a new instance of the BoxAgentComponent class.
        /// </summary>
        public FlockAgentComponent()
          : base("Flock Agent 3D", "FlockAgent 3D",
              "Agent parameters",
              "YFAtools", "AgentBased")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Minimum velocity", "minV", "Minimum velocity for agent", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Maximum velocity", "MaxV", "Maximum velocity for agent", GH_ParamAccess.item, 8);
            pManager.AddPointParameter("Start point for agent","startPt","the point from which to start agents",GH_ParamAccess.list);
            //pManager[2].Optional = true;
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent", "A", "agent to flock", GH_ParamAccess.item);
            //pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);

        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //string info;
            double minVelocity = 4;
            double maxVelocity = 8;
            List<Point3d> points = new List<Point3d>();
            var agents = new List<FlockAgent>();
            //Get values from grasshopper
            DA.GetData("Minimum velocity", ref minVelocity);
            DA.GetData("Maximum velocity", ref maxVelocity);
            DA.GetDataList("Start point for agent", points);
            //Assign velocity to points
            foreach (Point3d point in points)
            {
                FlockAgent agent = new FlockAgent(point, VectorOperations.GetRandomUnitVector() * minVelocity);
                agent.StartPosition = point;
                agent.MinVelocity = minVelocity;
                agent.MaxVelocity = maxVelocity;
                agents.Add(agent);
            }
            //informations check
            //info = "values are" + agents[0].MinVelocity + " " + agents[0].MaxVelocity;
            //assigning data for export
            var a = agents;
            //Export data to grasshopper
            DA.SetDataList(0, a);
            //DA.SetData("Info", info);
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
                return Resources._190512_01_FlockSimulation3D;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b5810995-e61f-4cd5-85c9-824cb6200324"); }
        }
    }
}