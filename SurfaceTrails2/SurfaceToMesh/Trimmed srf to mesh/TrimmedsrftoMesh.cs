using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.SurfaceToMesh.Trimmed_srf_to_mesh
{
    public class TrimmedsrftoMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TrimmedsrftoMesh class.
        /// </summary>
        public TrimmedsrftoMesh()
          : base("TrimmedsrftoMesh", "TrimmedsrftoMesh",
              "Description",
              "YFAtools", "SurfaceToMesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "trimmed surface to be meshed", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "u", "U paramter for mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V", "v", "V paramter for mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            Brep srfBrep = null;
            int u = 1;
            int v = 1;
            if (!DA.GetData(0, ref srfBrep)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;

            Mesh mesh = new Mesh();
            var srfPt = srfBrep.DuplicateVertices();
            var srf = NurbsSurface.CreateFromCorners(srfPt[0], srfPt[1], srfPt[2], srfPt[3]);

            for (int i = 0; i < u; i++)
            {
                for (int j = 0; j < v; j++)
                {
                    Mesh subMsh = new Mesh();

                    subMsh.Vertices.Add(srf.PointAt(srf.Domain(0).Length / u * i, srf.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(srf.PointAt(srf.Domain(0).Length / u * (i + 1), srf.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(srf.PointAt(srf.Domain(0).Length / u * (i + 1), srf.Domain(1).Length / v * (j + 1)));
                    subMsh.Vertices.Add(srf.PointAt(srf.Domain(0).Length / u * i, srf.Domain(1).Length / v * (j + 1)));

                    subMsh.Faces.AddFace(0, 1, 2, 3);

                    mesh.Append(subMsh);
                }
            }

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
            get { return new Guid("bba6b195-ba54-4fde-856d-827819fbc1d6"); }
        }
    }
}