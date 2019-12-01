using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
//This component controls the attracting Curve behaviour for the the flock
namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class RepellerCurveBehaviourComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AttractorCurveBehaviourComponent class.
        /// </summary>
        public RepellerCurveBehaviourComponent()
          : base("Repeller Curve Behaviour", "RepellerCurve",
              "controls the repelling Curve behaviour for the the flock",
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
            pManager.AddCurveParameter("Curves", "C", "Curves to attract agents", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "M", "strength of the behaviour", GH_ParamAccess.item, 1);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RepellerCurveBehaviour", "B", "Repeller Curve Behaviour to supply to container input in flocking engine",
                GH_ParamAccess.list);
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
            RepellerCurve repellerCurve = new RepellerCurve();
            List<Curve> curves = new List<Curve>();
            double multiplier = 1.0;
            //get values from grasshopper
            DA.GetDataList("Curves", curves);
            DA.GetData("Multiplier", ref multiplier);
            // ===============================================================================================
            // Applying Values to Class
            // ===============================================================================================
            repellerCurve.Curves = curves;
            repellerCurve.Multiplier = multiplier;
            // ===============================================================================================
            // Exporting Data to Grasshopper
            // ===============================================================================================
            DA.SetData("RepellerCurveBehaviour", repellerCurve);
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
                return Resources.RepellerCurveBehaviour;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cd785ef9-1762-49a9-80bf-77f0c0519066"); }
        }
    }
}