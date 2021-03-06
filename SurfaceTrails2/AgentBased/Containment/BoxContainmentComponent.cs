﻿using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This Component controls containment in a box boundary
namespace SurfaceTrails2.AgentBased.Containment
{
    public class BoxContainmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BoxContainmentComponent class.
        /// </summary>
        public BoxContainmentComponent()
          : base("Box Containment", "BoxContainment",
              "controls containment in a box boundary",
              "Zebra",
              "AgentBased")
        {
        }
        //Controls Place of component on grasshopper menu
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "Box container in which the flock will kept", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Strength of parameter", GH_ParamAccess.item, 1);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BoxContainer", "C", "Box container class to supply to container input in flocking engine"
                , GH_ParamAccess.item);
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
            BoxContainment container = new BoxContainment();
            Box box = Box.Unset;
            double multiplier = 1.0;
            //get values from grasshopper
            DA.GetData("Box",ref box);
            DA.GetData("Multiplier", ref multiplier);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            container.Box = box;
            container.Multiplier = multiplier;
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetData("BoxContainer", container);
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
                return Resources.BoxContainer;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2569696c-4157-4095-aaa3-9c34942091b0"); }
        }
    }
}