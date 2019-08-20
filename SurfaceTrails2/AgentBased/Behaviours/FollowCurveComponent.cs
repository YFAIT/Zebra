using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

//This component controls the Follow Curve behaviour for the the flock

namespace SurfaceTrails2.AgentBased.Behaviours
{
    public class FollowCurveComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FollowOrganizedPointsComponent class.
        /// </summary>
        public FollowCurveComponent()
          : base("FollowCurveBehaviour", "Nickname",
              "Description",
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
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Count", "C", "Curve resolution Point Count", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("Attract to Nearest Point", "N", "attracts agents to nearest point to their start point", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Loop", "L", "Loop", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FollowCurveBehaviour", "B", "FollowCurveBehaviour", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FollowCurve followCurve = new FollowCurve();
            List<Curve> curves = new List<Curve>();
            List<Circle> circles = new List<Circle>();
            double multiplier = 1.0;
            bool loop = true;
            int count = 0;
            //bool attractToNearestPt = true;

            DA.GetDataList("Curves", curves);
            DA.GetData("Count", ref count);
            DA.GetData("Multiplier", ref multiplier);
            DA.GetData("Loop", ref loop);
            //DA.GetData("Attract to Nearest Point", ref attractToNearestPt);


            Point3d[] ptArray;
            curves[0].DivideByCount(count, true, out ptArray);
            var points = ptArray.ToList();
            foreach (Point3d point in points)
                circles.Add(new Circle(point,1));

            followCurve.Circles = circles;
            followCurve.Multiplier = multiplier;
            followCurve.Loop = loop;
            //followCurve.Label = 'l';
            //follow.AttractToNearestPt = attractToNearestPt;
            //var info = "values are" + circles[0].Radius;
            DA.SetData("FollowCurveBehaviour", followCurve);
        }

        /// <summary>
        /// Provides an Icon for the component
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.FollowCurve_01;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("55131d56-de13-4122-90f3-134d7d95ff3a"); }
        }
    }
}