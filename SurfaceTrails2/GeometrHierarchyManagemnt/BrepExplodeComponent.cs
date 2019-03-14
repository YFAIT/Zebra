using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.GeometrHierarchyManagemnt
{
    public class BrepExplodeComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BrepExplodeComponent class.
        /// </summary>
        public BrepExplodeComponent()
          : base("BrepExplode", "BrepExplode",
              "Explodes breps while respecting data tree structure",
              "YFAtools", "GeometrHierarchyManagemnt")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "b", "Brep to explode", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Brep Faces", "F", "Exploded brep Faces", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Brep Edges", "E", "Exploded brep edges", GH_ParamAccess.tree);
            pManager.AddPointParameter("Brep Vertices", "V", "Exploded brep Vertices", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var breps = new List<Brep>();
            //Calling data from grasshopper
            if (!DA.GetDataList(0, breps)) return;
            //Assigning variables to classes
            var a= BrepExplode.BrepFaces(breps);
            var b= BrepExplode.BrepEdges(breps);
            var c= BrepExplode.BrepVertices(breps);
            //Exporting data back to grasshopper
            DA.SetDataTree(0, a);
            DA.SetDataTree(1, b);
            DA.SetDataTree(2, c);
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
            get { return new Guid("eebb1b42-83b2-49dd-ab9f-5baf64de5ba9"); }
        }
    }
}