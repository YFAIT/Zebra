using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.Crv
{
    public class _9_10_18_RatioRectangle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _9_10_18_RatioRectangle class.
        /// </summary>
        public _9_10_18_RatioRectangle()
          : base("Ratio Rectangle", "RatioRect",
              "Makes Rectangle from ratio and area",
              "Zebra", "Crv")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "l", "Length of the rectangle", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Width", "w", "Width of the rectangle", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Area", "a", "Desired area for the rectangle", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangle", "r", "Created rectangle", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "a", "area of the rectangle", GH_ParamAccess.item);
            pManager.AddPointParameter("Center", "c", "Center of the rectangle", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double length = 1.0;
            double width = 1.0;
            double area = 10;
            //get values from grasshopper
            if (!DA.GetData(0, ref length)) return;
            if (!DA.GetData(1, ref width)) return;
            if (!DA.GetData(2, ref area)) return;

            double u = length / width;
            double v = Math.Sqrt(u * area);
            double w = v / u;
            Rectangle3d rect = new Rectangle3d(Plane.WorldXY, Math.Ceiling(v), Math.Ceiling(w));
            //values for export
            var   r = new GH_Rectangle(rect);
            var   a = rect.Area;
            var   c = rect.Center;
            //Export data to grasshopper
            DA.SetData(0, r);
            DA.SetData(1, a);
            DA.SetData(2, c);
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
                return Resources._9_10_18_RatioRectangle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2e2c81b6-0a60-4047-b4a5-3381f9b5059a"); }
        }
    }
}