﻿using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SurfaceTrails2.Properties;
/*This component records any type of data and outputs list of it,
 useful for any recursive operation save data from each iteration seperately
 */
namespace SurfaceTrails2.Utilities
{
    public class ResettableDataRecorderComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ResettableDataRecorderComponent class.
        /// </summary>
        public ResettableDataRecorderComponent()
          : base("Resettable Data Recorder", "ResettableRecorder",
              "This component records any type of data and outputs list of it," +
              " useful for any recursive operation save data from each iteration seperately",
              "Zebra", "Utilities")
        {
        }
        GH_Structure<IGH_Goo> tree = new GH_Structure<IGH_Goo>();
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to be recorded", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Reset", "R", "reset data stored in recorder if true", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear", "C", "Clears data stored in recorder if true", GH_ParamAccess.item);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Recorded Data", GH_ParamAccess.tree);
            //pManager[0].DataMapping = GH_DataMapping.Graft;
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
            bool reset = false;
            bool clear = true;
            GH_Structure<IGH_Goo> data;
            //get values from grasshopper
            DA.GetDataTree("Data",out data);
            DA.GetData("Reset", ref reset);
            DA.GetData("Clear", ref clear);
// ===============================================================================================
// Applying Values to Class
// ===============================================================================================
            if (reset == true || clear == true)
                tree.Clear();
            else
            {
                for (int i = 0; i < data.PathCount; i++)
                {
                    var list = data.get_Branch(i);
                    for (var j = 0; j < list.Count; j++)
                    {
                        var item = list[j];
                        tree.Append(item as IGH_Goo, new GH_Path(i,j));
                    }
                }
            }
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            var a = tree;
            DA.SetDataTree(0, a);
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
                return Resources.Rec__2_;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b33a25fb-fec5-4d55-ad71-8e0a19f89e35"); }
        }
    }
}