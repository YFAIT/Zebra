using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace SurfaceTrails2.Utilities
{
    static class MeshOperations
    {
        //Find the center of a mesh face
        public static Point3d MeshFaceCenter(int meshfaceindex, Mesh m)
        {
            var temppt = new Point3d(0, 0, 0);

            temppt.X += m.Vertices[m.Faces[meshfaceindex].A].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].A].Y;
            temppt.Z += m.Vertices[m.Faces[meshfaceindex].A].Z;

            temppt.X += m.Vertices[m.Faces[meshfaceindex].B].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].B].Y;
            temppt.Z += m.Vertices[m.Faces[meshfaceindex].B].Z;

            temppt.X += m.Vertices[m.Faces[meshfaceindex].C].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].C].Y;
            temppt.Z += m.Vertices[m.Faces[meshfaceindex].C].Z;

            if (m.Faces[meshfaceindex].IsQuad)
            {
                temppt.X += m.Vertices[m.Faces[meshfaceindex].D].X;
                temppt.Y += m.Vertices[m.Faces[meshfaceindex].D].Y;
                temppt.Z += m.Vertices[m.Faces[meshfaceindex].D].Z;

                temppt.X /= 4;
                temppt.Y /= 4;
                temppt.Z /= 4;
            }
            else
            {
                temppt.X /= 3;
                temppt.Y /= 3;
                temppt.Z /= 3;
            }

            return temppt;
        }
        //Move mesh vertex 
        public static Mesh VertexMove(Mesh meshIn, Point3d attrPt, int cPCount, Vector3d subVec, double vAmp,
            bool toggle)
        {
            var vList = meshIn.Vertices;

            List<Point3f> vertToList = vList.ToList();
            List<Point3d> vertPtList = vertToList.ConvertAll(x => (Point3d)x); //convert vertexlist from point3f to point3d
            List<Point3d> vertPtDup = new List<Point3d>(vertPtList);
            List<Point3d> vertAsPoint = new List<Point3d>();

         
                for (int i = 0; i < cPCount; i++)
                { //for however many closestpoints you want each attractor to have

                    int cIndex_A = Rhino.Collections.Point3dList.ClosestIndexInList(vertPtList, attrPt);
                    vertAsPoint.Add(vertPtList[cIndex_A]);

                    //        Vector3d subVec = Vector3d.Subtract(new Vector3d(pt), new Vector3d(vertPtList[cIndex_A]));
                    //        subVec.Unitize();

                    Vector3d mult = new Vector3d();
                    if (!toggle)
                    {
                        mult = Vector3d.Multiply(subVec, vAmp); //if toggle is false amplify the vector by the user defined value
                    }
                    else
                    {
                        double dist = attrPt.DistanceTo(vertPtList[cIndex_A]);
                        mult = Vector3d.Multiply(subVec, dist); //if toggle is true then use the distance to the creeper pos
                    }

                    int moveIndex = vertPtDup.IndexOf(vertPtList[cIndex_A]); //get the index of the current closest point

                    vList.SetVertex(moveIndex, Point3d.Add(mult, vertPtList[cIndex_A])); //set the mesh vertex at the specified index to the new location

                    vertPtList.RemoveAt(cIndex_A); //remove the previous index from the vertex list to find the next closest point
                }
            
            return meshIn;
        }
    }
}
