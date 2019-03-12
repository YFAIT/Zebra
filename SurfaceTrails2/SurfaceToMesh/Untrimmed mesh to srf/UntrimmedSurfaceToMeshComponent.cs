using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SurfaceTrails2.Untrimmed_mesh_to_srf
{
    public class UntrimmedSurfaceToMeshComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public UntrimmedSurfaceToMeshComponent()
          : base("UntrimmedQuadToMesh", "UntrimmedToMesh",
              "A remake to the old school componenet Mesh surface but with less parameters",
              "YFAtools", "SurfaceToMesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "Untrimmed surface to be meshed", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "u", "U paramter for mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V", "v", "V paramter for mesh", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "m", "Output Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface surface = null;
            int u = 1;
            int v = 1;
            if (!DA.GetData(0, ref surface)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;

            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();


            for (int i = 0; i < u; i++)
            {
                for (int j = 0; j < v; j++)
                {
                    Rhino.Geometry.Mesh subMsh = new Rhino.Geometry.Mesh();

                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * i, surface.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * (i + 1), surface.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * (i + 1), surface.Domain(1).Length / v * (j + 1)));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * i, surface.Domain(1).Length / v * (j + 1)));

                    subMsh.Faces.AddFace(0, 1, 2, 3);

                    mesh.Append(subMsh);
                }
            }

            mesh.Vertices.CombineIdentical(true, true);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();

            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f37c638e-0869-418f-976d-5fb5ded2150a"); }
        }
    }
}
