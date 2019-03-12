using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace SurfaceTrails2
{
    public class Composite : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Composite class.
        /// </summary>
        public Composite()
          : base("Composite Lines", "Composite",
              "Creates YFA composite lines with respect to it's thickness",
              "YFAtools", "Composite")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "b", "Brep to make the YFA composite system on", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Naked edge length", "length", "Length of edge on the naked sides of the brep", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Clothed edge width", "width", "width of edge on the clothed sides of the brep", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Center Mark", "Center", "Center Mark scale", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Composite thickness", "Thickness", "Thickness of YFA composite", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddCurveParameter("Center Mark", "Center","Centermark of the composite cells",GH_ParamAccess.item);
            //pManager.AddCurveParameter("Nails Locations","Nails","Location of nails on YFA composite",GH_ParamAccess.item);
            //pManager.AddCurveParameter("Weaving Paths", "curves", "Weaving paths of the YFA composite", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("srf", "srf", "srf", GH_ParamAccess.tree);
            pManager.AddCurveParameter("crv", "crv", "crv", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps2 = new List<Brep>();
            Brep brep = null;
            double nakedLength = 5;
            double clothedWidth = 1;
            double centerMark = 1;
            double thickness = 2;
            if (!DA.GetDataList(0, breps2)) return;
            if (!DA.GetData(1, ref nakedLength)) return;
            if (!DA.GetData(2, ref clothedWidth)) return;
            if (!DA.GetData(3, ref centerMark)) return;
            if (!DA.GetData(4, ref thickness)) return;

            var faces =  BrepExplode.BrepFaces(breps2);
         var edges =   BrepExplode.BrepEdges(breps2);


            Curve[] polyline = Curve.JoinCurves(edges);



                var a = faces;
                var b = edges;

                DA.SetDataTree(0, a);
                DA.SetDataTree(1, b);


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
            get { return new Guid("3bd8113a-e9f6-4101-9d2b-e924d8d899c8"); }
        }
    }
}