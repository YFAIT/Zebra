using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased
{
    //Interface for agents to be contained in any geometry it varies based on geometry type
    public interface IAgentContainment
    {
        Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity);
    }
    //contain agent in a box
     class BoxContainment : IAgentContainment
    {
        public Box Box { get; set; }
        public  Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity)
        {
            double boundingBoxMinX = Box.PointAt(0, 0, 0).X;
            double boundingBoxMinY = Box.PointAt(0, 0, 0).Y;
            double boundingBoxMinZ = Box.PointAt(0, 0, 0).Z;
            double boundingBoxMaxX = Box.PointAt(1, 1, 1).X;
            double boundingBoxMaxY = Box.PointAt(1, 1, 1).Y;
            double boundingBoxMaxZ = Box.PointAt(1, 1, 1).Z;
            double multiplier = 3;

            if (position.X < boundingBoxMinX)
                desiredVelocity += new Vector3d(boundingBoxMaxX - position.X, 0, 0) * multiplier;

            else if (position.X > boundingBoxMaxX)
                desiredVelocity += new Vector3d(-position.X, 0, 0) * multiplier;

            if (position.Y < boundingBoxMinY)
                desiredVelocity += new Vector3d(0, boundingBoxMaxY - position.Y, 0) * multiplier;

            else if (position.Y > boundingBoxMaxY)
                desiredVelocity += new Vector3d(0, -position.Y, 0) * multiplier;

            if (position.Z < boundingBoxMinZ)
                desiredVelocity += new Vector3d(0, 0, boundingBoxMaxZ - position.Z) * multiplier;

            else if (position.Z > boundingBoxMaxZ)
                desiredVelocity += new Vector3d(0, 0, -position.Z) * multiplier;

            return desiredVelocity;
        }
    }
    //contain agent in a brep
    class BrepContainment : IAgentContainment
    {
        public Mesh Mesh { get; set; }
        public Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity)
        {
            double multiplier = 3;

            if (!Mesh.IsPointInside(position, 0.01, false))
            {
                var reverse = Point3d.Subtract(position, Mesh.ClosestPoint(position));
                reverse.Reverse();
                 desiredVelocity += reverse * multiplier;
            }
            return desiredVelocity;
        }
    }
    //contain agent in a mesh (fast)
    class MeshContainment : IAgentContainment
    {
        public Mesh Mesh { get; set; }

        public Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity)
        {
        double multiplier = 40;

            if (!Mesh.IsPointInside(position, 0.01, false))
            {
                var reverse = Point3d.Subtract(position, Mesh.ClosestPoint(position));
                reverse.Reverse();
                desiredVelocity += reverse * multiplier;
            }
            return desiredVelocity;
        }
    }
    //contain agent in a Surface
    class SurfaceContainment : IAgentContainment
    {
        public NurbsSurface Surface { get; set; }
        public double xMin { get; set; }
        public double xMax { get; set; }
        public double yMin { get; set; }
        public double yMax { get; set; }

        public Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity)
        {
            var bounceMultiplier = 10;

            //var boundingBox = Surface.GetBoundingBox(true);
            //var xMin = boundingBox.Corner(true, true, true).X;
            //var xMax = boundingBox.Corner(false, true, true).X;
            //var yMin = boundingBox.Corner(true, true, true).Y;
            //var yMax = boundingBox.Corner(true, false, true).Y;

            if (Position.X < xMin)
                desiredVelocity += new Vector3d((xMax - Position.X) * bounceMultiplier, 0.0, 0.0);

            else if (Position.X > xMax)
                desiredVelocity += new Vector3d(-Position.X * bounceMultiplier, 0.0, 0.0);


            if (Position.Y < yMin)
                desiredVelocity += new Vector3d(0.0, (yMax - Position.Y) * bounceMultiplier, 0.0);

            else if (Position.Y > yMax)
                desiredVelocity += new Vector3d(0.0, (-Position.Y) * bounceMultiplier, 0.0);

            return desiredVelocity;
        }
    }
    //contain agent in a plane
    class PlaneContainment : IAgentContainment
    {
        public Curve Curve { get; set; }
        public Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity )
        {
            double multiplier = 80;
            if (Curve.Contains(position) == PointContainment.Outside || Curve.Contains(position) == PointContainment.Coincident)
            {
                double t;
                Curve.ClosestPoint(position, out t);
                var reverse = Point3d.Subtract(position, Curve.PointAt(t));
                reverse.Reverse();
                desiredVelocity = reverse * multiplier;
            }
            return desiredVelocity;
        }
    }
    //contain agent in a mesh for forming with wind
   public class MeshWindForming : IAgentContainment
    {
        public Mesh Mesh { get; set; }
        

        public Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity)
        {
        var newMesh = Mesh.DuplicateMesh();
        double multiplier = 40;

            if (newMesh.IsPointInside(position, 0.01, false))
            {
                var reverse = Point3d.Subtract(position, newMesh.ClosestPoint(position));
                reverse.Reverse();
                desiredVelocity += reverse * multiplier;
                
                Mesh = MeshOperations.VertexMove(newMesh, position, 1, desiredVelocity, 0.5, false);
            }
            return desiredVelocity;
        }
    }
}
