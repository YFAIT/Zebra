using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.AgentBased
{
    public class _30_4_19_FlockProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _30_4_19_FlockProperties class.
        /// </summary>
        public _30_4_19_FlockProperties()
          : base("FlockProperties", "FProps",
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
            pManager.AddIntegerParameter("Count", "C", "Number of Agents", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Timestep", "T", "Timestep", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Neighbourhood Radius", "R", "Neighbourhood Radius", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("Alignment", "A", "Alignment", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Cohesion", "C", "Cohesion", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation", "S", "Separation", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation Distance", "D", "Separation Distance", GH_ParamAccess.item, 1.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Flock Values", "F", "Flock Values", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iCount = 0;
            double iTimestep = 0.0;
            double iNeighbourhoodRadius = 0.0;
            double iAlignment = 0.0;
            double iCohesion = 0.0;
            double iSeparation = 0.0;
            double iSeparationDistance = 0.0;
            List<double> flockProps = new List<double>();
            //var prop = new FlockProperties();

            DA.GetData("Count", ref iCount);
            DA.GetData("Timestep", ref iTimestep);
            DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius);
            DA.GetData("Alignment", ref iAlignment);
            DA.GetData("Cohesion", ref iCohesion);
            DA.GetData("Separation", ref iSeparation);
            DA.GetData("Separation Distance", ref iSeparationDistance);
       
            flockProps.Add(iCount);
            flockProps.Add(iTimestep);
            flockProps.Add(iNeighbourhoodRadius);
            flockProps.Add(iAlignment);
            flockProps.Add(iCohesion);
            flockProps.Add(iSeparation);
            flockProps.Add(iSeparationDistance);

            DA.SetDataList("Flock Values", flockProps);
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