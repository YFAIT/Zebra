using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.Mesh
{
    public class MeshIterativeMove : GH_Component
    {

        public System.Windows.Forms.Timer timer;
        public int counter;
        int maxCounter, interval;
        private bool reset, run;


        public void Start()
        {
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
        public void Reset()
        {
            counter = 0;
        }
        public void Update()
        {
            // DoSomethingEpic 
            counter++;
        }

        public void UpdateSolution(object source, EventArgs e)
        {
            Update();
            ExpireSolution(true);
        }

        protected override void ExpireDownStreamObjects()
        {
            if (run)
            {
                this.Params.Output[0].Recipients[2].ExpireSolution(false);
            }
        }


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MeshIterativeMove()
          : base("Mesh Iterative move Attractor", "MeshIterativeMove",
              "Moves mesh's closest vertices to attractor point iteratively",
              "YFAtools", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "run the Component", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "Rst", "Reset timer to 0", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("TimeInterval", "Int", "The time interval between iteration in milliseconds",GH_ParamAccess.item, 500);
            pManager.AddIntegerParameter("MaximumIterations", "Max", "Maximum number of Iterations", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Counter", "C", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //I would not leave the SolveInstance without making sure that the timer is stopped... but well
            if (!DA.GetData(0, ref run)) return;
            if (!DA.GetData(1, ref reset)) return;
            if (!DA.GetData(2, ref interval)) return;
            if (!DA.GetData(3, ref maxCounter)) return;

            if (timer == null )
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = interval;
                timer.Tick += UpdateSolution;
            }

            if (reset)
            {
                Reset();
                timer.Interval = interval;
            }

            if (run && !timer.Enabled)
            {
                Start();
            }
            else if (!run || timer.Enabled && maxCounter != 0 && counter >= maxCounter)
            {
                Stop();
            }

            DA.SetData(0, counter);
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
            get { return new Guid("29166f33-d4c0-4e40-9f9a-556b2d251760"); }
        }
    }
}