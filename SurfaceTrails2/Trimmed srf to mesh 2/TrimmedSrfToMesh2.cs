using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.Trimmed_srf_to_mesh_2
{
    public class TrimmedSrfToMesh2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TrimmedSrfToMesh2 class.
        /// </summary>
        public TrimmedSrfToMesh2()
          : base("TrimmedSrfToMesh2", "TrimmedSrfToMesh2",
              "Description",
              "YFAtools", "SurfaceToMesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "Untrimmed surface to be meshed", GH_ParamAccess.item);
        
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "m", "Output Mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "p", "Output points", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep surface = null;
            if (!DA.GetData(0, ref surface)) return;

            Mesh mesh = new Mesh();

            var srfPt = surface.DuplicateVertices();

            Mesh subMsh = new Mesh();

            subMsh.Vertices.Add(srfPt[0]);
            subMsh.Vertices.Add(srfPt[1]);
            subMsh.Vertices.Add(srfPt[2]);
            subMsh.Vertices.Add(srfPt[3]);

            subMsh.Faces.AddFace(0, 1, 2, 3);

            mesh.Append(subMsh);


            mesh.Vertices.CombineIdentical(true, true);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();

            DA.SetData(0, mesh);
            DA.SetDataList(1, srfPt);

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
            get { return new Guid("14da5ede-fcda-45dd-bd86-4c43a2a30e1a"); }
        }
    }
}