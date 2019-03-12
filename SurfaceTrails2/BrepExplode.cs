using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace SurfaceTrails2.Composite
{
    public static class BrepExplode
    {

        public static DataTree<Curve> BrepEdges(List<Brep> breps)
        {
            DataTree<Curve> faceEdges = new DataTree<Curve>();

            int i = 0;

            foreach (Brep brep in breps)
            {
                int j = 0;

                foreach (var face in brep.Faces)
                {
                    Brep faceBrep = face.DuplicateFace(true);
                    var crvs = faceBrep.DuplicateEdgeCurves();

                    for (int k = 0; k < crvs.Length; k++)
                    {
                        GH_Path path = new GH_Path(i, j);
                        faceEdges.Add(crvs[k], path);
                    }
                    j++;
                }

                i++;
            }
            return faceEdges;
        }

        public static DataTree<Brep> BrepFaces(List<Brep> breps)
        {
            DataTree<Brep> faceBreps = new DataTree<Brep>();
            int i = 0;

            foreach (Brep brep in breps)
            {
                foreach (var face in brep.Faces)
                {
                    Brep faceBrep = face.DuplicateFace(true);
                    GH_Path path = new GH_Path(i);

                    faceBreps.Add(faceBrep,path);
                }

                i++;
            }
            return faceBreps;
        }
    }
}



