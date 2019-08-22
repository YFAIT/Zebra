using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.OperationLibrary;
using SurfaceTrails2.Properties;
//this component controls the flock starting position and velocity on a surface
namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public class FlockAgentSrfComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FlockAgent2DComponent class.
        /// </summary>
        public FlockAgentSrfComponent()
          : base("Flock Agent On Surface", "FlockAgentSrf",
              "controls the flock starting position and velocity on a surface",
              "Zebra", "AgentBased")
        {
        }
        //Controls Place of component on grasshopper menu
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Minimum velocity", "minV", "Minimum velocity for agent", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Maximum velocity", "MaxV", "Maximum velocity for agent", GH_ParamAccess.item, 8);
            pManager.AddPointParameter("Start point for agent", "startPt", "Initial position for agents", GH_ParamAccess.list);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent", "A", "Agent to flock", GH_ParamAccess.item);
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
            //string info;
            double minVelocity = 4;
            double maxVelocity = 8;
            List<Point3d> points = new List<Point3d>();
            var agents = new List<FlockAgent>();
            const double xMin = 0;
            const double xMax = 30;
            const double yMin = 0;
            const double yMax = 30;
            //Get values from grasshopper
            DA.GetData("Minimum velocity", ref minVelocity);
            DA.GetData("Maximum velocity", ref maxVelocity);
            DA.GetDataList("Start point for agent", points);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            BoundingBox box = new BoundingBox(points);
            //Assign velocity to points
            foreach (Point3d point in points)
            {
                var nu = NumberOperations.remap(box.Min.X,
                    box.Max.X, xMin, xMax, point.X);
                var nv = NumberOperations.remap(box.Min.Y,
                    box.Max.Y, yMin, yMax, point.Y);
                var remappedPoint = new Point3d(nu, nv, 0);

                FlockAgent agent =
                    new FlockAgent(remappedPoint, VectorOperations.GetRandomUnitVectorXY() * minVelocity)
                    {
                        StartPosition = remappedPoint,
                        MinVelocity = minVelocity,
                        MaxVelocity = maxVelocity
                    };
                agents.Add(agent);
            }
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            //assigning data for export
            var a = agents;
            //Export data to grasshopper
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
                return Resources._190512_01_FlockSimulation_Srf;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5a2ce7c1-65b9-4f81-b62c-b4dde1feacdc"); }
        }
    }
}