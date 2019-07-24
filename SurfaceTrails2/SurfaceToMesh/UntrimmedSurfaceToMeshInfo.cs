using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SurfaceTrails2.SurfaceToMesh.Untrimmed_mesh_to_srf
{
    public class UntrimmedSurfaceToMeshInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SurfaceTrails2";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("a8ddccf9-2c20-4b7b-a0e3-a531b52bfcc9");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Microsoft";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
