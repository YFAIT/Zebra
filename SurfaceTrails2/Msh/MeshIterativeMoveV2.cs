using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.Mesh
{
    public class MeshIterativeMove : GH_Component
    {
        private GH_Document ghDocument;
        public Timer timer = new Timer();
        int maxCounter, interval;
        public int counter;
        private bool reset, run;
        
        void documentSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            ghDocument.SolutionEnd -= documentSolutionEnd;
            timer.Interval = interval;
            timer.Tick += timerTick;
            timer.Start();
        }
        void timerTick(object sender, EventArgs e)
        {
            timer.Tick -= timerTick;
            timer.Stop();
            ghDocument.NewSolution(true);
        }

        public MeshIterativeMove()
          : base("Mesh Iterative move Attractor", "MeshIterativeMove",
              "Moves mesh's closest vertices to attractor point iteratively",
              "YFAtools", "Mesh")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "run the Component", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "Rst", "Reset timer to 0", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("TimeInterval", "Int", "The time interval between iteration in milliseconds",GH_ParamAccess.item, 500);
            pManager.AddIntegerParameter("MaximumIterations", "Max", "Maximum number of Iterations", GH_ParamAccess.item, 0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Counter", "C", "", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //I would not leave the SolveInstance without making sure that the timer is stopped... but well
            if (!DA.GetData(0, ref run)) return;
            if (!DA.GetData(1, ref reset)) return;
            if (!DA.GetData(2, ref interval)) return;
            if (!DA.GetData(3, ref maxCounter)) return;

            if (reset)
                counter = 0;

            if (run && !timer.Enabled)
            {
                timer.Start();
                ghDocument = OnPingDocument();
                ghDocument.SolutionEnd += documentSolutionEnd;
            }
            else if (!run || timer.Enabled && maxCounter != 0 && counter >= maxCounter)
                timer.Stop();

            counter++;
            DA.SetData(0, counter);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("29166f33-d4c0-4e40-9f9a-556b2d251760"); }
        }
    }
}