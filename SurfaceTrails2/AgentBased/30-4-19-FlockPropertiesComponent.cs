using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using SurfaceTrails2.Properties;

/*This Class Control all changable parameters in form of sliders for ease of use
Then adds them to the main component of flocking
*/
namespace SurfaceTrails2.AgentBased
{
    public class _30_4_19_FlockProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _30_4_19_FlockProperties class.
        /// </summary>
        public _30_4_19_FlockProperties()
          : base("FlockProperties", "FProps",
              "Controls agent properties and flocking paramerties to get different movement patterns",
              "Zebra", "AgentBased")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Timestep", "T", "Controls the speed of the flock", GH_ParamAccess.item, 0.026);
            pManager.AddNumberParameter("Neighbourhood Radius", "R", "Controls the radius of the flock", GH_ParamAccess.item, 0.88);
            pManager.AddNumberParameter("Alignment", "A", "Strength of going towards a certain goal", GH_ParamAccess.item, 0.54);
            pManager.AddNumberParameter("Cohesion", "C", "Strength of agents staying close to each other", GH_ParamAccess.item, 19.73);
            pManager.AddNumberParameter("Separation", "S", "Strength of agents not hitting each other", GH_ParamAccess.item, 30.00);
            pManager.AddNumberParameter("Separation Distance", "D", "Distance between agents", GH_ParamAccess.item, 0.27);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Flock Properties", "F", "Contains parameters to control the flock", GH_ParamAccess.list);
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
            double iTimestep = 0.0;
            double iNeighbourhoodRadius = 0.0;
            double iAlignment = 0.0;
            double iCohesion = 0.0;
            double iSeparation = 0.0;
            double iSeparationDistance = 0.0;
            List<double> flockProps = new List<double>();
            //get values from grasshopper
            DA.GetData("Timestep", ref iTimestep);
            DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius);
            DA.GetData("Alignment", ref iAlignment);
            DA.GetData("Cohesion", ref iCohesion);
            DA.GetData("Separation", ref iSeparation);
            DA.GetData("Separation Distance", ref iSeparationDistance);
// ===============================================================================================
// Encapsulating parameters in container
// ===============================================================================================
            flockProps.Add(iTimestep);
            flockProps.Add(iNeighbourhoodRadius);
            flockProps.Add(iAlignment);
            flockProps.Add(iCohesion);
            flockProps.Add(iSeparation);
            flockProps.Add(iSeparationDistance);
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetDataList("Flock Properties", flockProps);
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
                return Resources.Properties;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("73aac28f-0f92-41f2-9315-8d3f26dea4cb"); }
        }
    }
}