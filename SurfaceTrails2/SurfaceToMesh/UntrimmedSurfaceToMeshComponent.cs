using System;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.SurfaceToMesh
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
              "Zebra", "SurfaceToMesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "Untrimmed surface to be meshed", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "u", "U paramter for mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V", "v", "V paramter for mesh", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            Mesh mesh = new Mesh();
            //get values from grasshopper and apply them to variables
            if (!DA.GetData(0, ref surface)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            //transform quad untrimmed surface to mesh
            for (int i = 0; i < u; i++)
            {
                for (int j = 0; j < v; j++)
                {
                    Mesh subMsh = new Mesh();

                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * i, surface.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * (i + 1), surface.Domain(1).Length / v * j));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * (i + 1), surface.Domain(1).Length / v * (j + 1)));
                    subMsh.Vertices.Add(surface.PointAt(surface.Domain(0).Length / u * i, surface.Domain(1).Length / v * (j + 1)));

                    subMsh.Faces.AddFace(0, 1, 2, 3);
                    mesh.Append(subMsh);
                }
            }
            //combines meshes and unify faces and normals
            mesh.Vertices.CombineIdentical(true, true);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            //export data to grasshopper
            DA.SetData(0, mesh);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.UntrimmedQuadtoMesh;
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
