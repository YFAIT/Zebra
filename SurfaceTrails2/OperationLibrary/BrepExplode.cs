﻿using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
//This Class contains core methods to explode brep while keep its hierarchy
namespace SurfaceTrails2.OperationLibrary
{
    public static class BrepExplode
    {
// ===============================================================================================
// Get Vertices from breps
// ===============================================================================================
        public static DataTree<Point3d> BrepVertices(List<Brep> breps)
        {
            DataTree<Point3d> faceVertices = new DataTree<Point3d>();

            int i = 0;

            foreach (Brep brep in breps)
            {
                int j = 0;
                foreach (var face in brep.Faces)
                {
                    Brep faceBrep = face.DuplicateFace(true);
                    var pts = faceBrep.DuplicateVertices();

                    for (int k = 0; k < pts.Length; k++)
                    {
                        GH_Path path = new GH_Path(i, j);
                        faceVertices.Add(pts[k], path);
                    }
                    j++;
                }
                i++;
            }
            return faceVertices;
        }
// ===============================================================================================
// Get edges from breps
// ===============================================================================================
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
// ===============================================================================================
// Get Faces(surfaces) from breps
// ===============================================================================================
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



