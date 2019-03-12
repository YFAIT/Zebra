 using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.FlockingInBox;

namespace SurfaceTrails2.AgentBased.FlockingInBox
{
    public class FlockAgentComponent : GH_Component
    {
        double minVelocity = 4;
        double maxVelocity = 8;
        /// <summary>
        /// Initializes a new instance of the BoxAgentComponent class.
        /// </summary>
        public FlockAgentComponent()
          : base("Flock Agent", "FlockAgent",
              "Agent parameters",
              "YFAtools", "AgentBased")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddBooleanParameter("Reset", "Reset", "Reset Agents", GH_ParamAccess.item, false);
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
            pManager.AddGenericParameter("Agent", "Agent", "agent to flock", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
           
            List<Point3d> points = new List<Point3d>();
          

            DA.GetData("Reset", ref reset);
            DA.GetData("Minimum velocity", ref minVelocity);
            DA.GetData("Maximum velocity", ref maxVelocity);
            DA.GetDataList("Start point for agent", points);

            var agents = new List<FlockAgent>();


            foreach (Point3d point in points)
            {
                FlockAgent agent = new FlockAgent(point, Util.GetRandomUnitVector() * minVelocity);
                agent.MinVelocity = minVelocity;
                agent.MaxVelocity = maxVelocity;
              
                agents.Add(agent);
            }
            //if (reset)
            //{
            //agents.Clear();
            int i = 0;
                foreach (Point3d point in points)
                {
                    if (/*agents[i].Position != point ||*/ reset)
                    {
                        //agents.Clear();
                        //break;
                        ExpireSolution(true);
                }
                   
                //FlockAgent agent = new FlockAgent(point, Util.GetRandomUnitVector() * minVelocity);
                //agent.MinVelocity = minVelocity;
                //agent.MaxVelocity = maxVelocity;
                //agents.Add(agent);
                i++;
            }
            //}
           



            var a = agents;

            DA.SetDataList(0, a);

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
                return null;
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