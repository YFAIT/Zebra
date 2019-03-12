using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.Mesh
{
    public class MeshIterativeMove : GH_Component
    {
        public MeshIterativeMove()
          : base("Timer Example",
            "TimEx",
            "Run an adjustable timer",
            "YFAtools", "Mesh")
        {
            Counter = 0;
            Maximum = 0;
            Running = false;
            Schedule = DateTime.MaxValue;
        }

        private int Counter { get; set; }
        private int Maximum { get; set; }
        private bool Running { get; set; }
        private DateTime Schedule { get; set; }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run the timer", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "Rst", "Reset counter to 0", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Interval", "Int", "timer interval in milliseconds", GH_ParamAccess.item, 500);
            pManager.AddIntegerParameter("Maximum", "Max", "Maximum count", GH_ParamAccess.item, 100);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Counter", "C", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess access)
        {
            bool running = false;
            bool reset = false;
            int maximum = 10;
            int interval = 500;

            if (!access.GetData(0, ref running)) return;
            if (!access.GetData(1, ref reset)) return;
            if (!access.GetData(2, ref interval)) return;
            if (!access.GetData(3, ref maximum)) return;

            if (interval < 10)
            {
                interval = 10;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Timer intervals must be 10 milliseconds or more.");
            }

            Running = running;
            Maximum = Math.Max(0, maximum);
            if (reset) Counter = 0;

            if (running)
                if (Counter <= Maximum)
                {
                    Schedule = DateTime.UtcNow + TimeSpan.FromMilliseconds(interval);
                    OnPingDocument()?.ScheduleSolution(interval, Callback);
                }

            access.SetData(0, Math.Min(Counter, Maximum));

            Counter++;
        }

        private void Callback(GH_Document doc)
        {
            // We've exceeded the maximum.
            // No further solutions from this object.
            if (Counter >= Maximum)
                return;

            // This callback *always* happens if we've scheduled a solution and
            // a new solution runs. Even if that new solution is much earlier than
            // the one we requested. If that's the case, i.e. if this callback is
            // called much too early, we need to schedule a new solution.
            var now = DateTime.UtcNow;
            if (now < Schedule)
            {
                // Yup, too early.
                var newDelay = Schedule - now;
                doc.ScheduleSolution((int)newDelay.TotalMilliseconds, Callback);
            }
            else
            {
                // Ok, this callback is close to or over our requested solution.
                ExpireSolution(false);
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("29166f33-d4c0-4e40-9f9a-556b2d251760"); }
        }
    }
}