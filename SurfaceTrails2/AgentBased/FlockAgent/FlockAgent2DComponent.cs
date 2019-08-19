using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
using SurfaceTrails2.Utilities;
//this component control the flock strating position and velocity in 2d mode
namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public class FlockAgent2DComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FlockAgent2DComponent class.
        /// </summary>
        public FlockAgent2DComponent()
          : base("FlockAgent2D", "Nickname",
              "Description",
              "YFAtools", "AgentBased")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Minimum velocity", "minV", "Minimum velocity for agent", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Maximum velocity", "MaxV", "Maximum velocity for agent", GH_ParamAccess.item, 8);
            pManager.AddPointParameter("Start point for agent", "startPt", "the point from which to start agents", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent", "A", "agent to flock", GH_ParamAccess.item);
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
                FlockAgent agent =
                    new FlockAgent(point, VectorOperations.GetRandomUnitVectorXY() * minVelocity)
                    {
                        StartPosition = point,
                        MinVelocity = minVelocity,
                        MaxVelocity = maxVelocity
                    };
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
                return Resources._190512_01_FlockSimulation_2d;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ba3fc0fc-de70-4ef5-9707-3fc0cd5821ba"); }
        }
    }
}