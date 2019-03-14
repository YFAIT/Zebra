using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased
{
    public interface IAgentContainment
    {
        Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity);
    }
     class BoxContainment : IAgentContainment
    {
        public Box Box { get; set; }
        public  Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity)
        {
            double boundingBoxMinX = Box.PointAt(0, 0, 0).X;
            double boundingBoxMinY = Box.PointAt(0, 0, 0).Y;
            double boundingBoxMinZ = Box.PointAt(0, 0, 0).Z;
            double boundingBoxMaxX = Box.PointAt(1, 1, 1).X;
            double boundingBoxMaxY = Box.PointAt(1, 1, 1).Y;
            double boundingBoxMaxZ = Box.PointAt(1, 1, 1).Z;
            double multiplier = 3;

            if (Position.X < boundingBoxMinX)
                desiredVelocity += new Vector3d(boundingBoxMaxX - Position.X, 0, 0) * multiplier;

            else if (Position.X > boundingBoxMaxX)
                desiredVelocity += new Vector3d(-Position.X, 0, 0) * multiplier;

            if (Position.Y < boundingBoxMinY)
                desiredVelocity += new Vector3d(0, boundingBoxMaxY - Position.Y, 0) * multiplier;

            else if (Position.Y > boundingBoxMaxY)
                desiredVelocity += new Vector3d(0, -Position.Y, 0) * multiplier;

            if (Position.Z < boundingBoxMinZ)
                desiredVelocity += new Vector3d(0, 0, boundingBoxMaxZ - Position.Z) * multiplier;

            else if (Position.Z > boundingBoxMaxZ)
                desiredVelocity += new Vector3d(0, 0, -Position.Z) * multiplier;

            return desiredVelocity;
        }
    }
    class BrepContainment : IAgentContainment
    {
        public Mesh Mesh { get; set; }
        public Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity)
        {
            double multiplier = 3;

            if (!Mesh.IsPointInside(Position, 0.01, false))
            {
                var reverse = Point3d.Subtract(Position, Mesh.ClosestPoint(Position));
                reverse.Reverse();
                 desiredVelocity += reverse * multiplier;
            }
            return desiredVelocity;
        }
    }
    class MeshContainment : IAgentContainment
    {
        public Mesh Mesh { get; set; }

        public Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity)
        {
        double multiplier = 40;

            if (!Mesh.IsPointInside(Position, 0.01, false))
            {
                var reverse = Point3d.Subtract(Position, Mesh.ClosestPoint(Position));
                reverse.Reverse();
                desiredVelocity += reverse * multiplier;
            }
            return desiredVelocity;
        }
    }

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

    class PlaneContainment : IAgentContainment
    {
        public Curve Curve { get; set; }
        public Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity )
        {
            double multiplier = 80;
            if (Curve.Contains(Position) == PointContainment.Outside || Curve.Contains(Position) == PointContainment.Coincident)
            {
                double t;
                Curve.ClosestPoint(Position, out t);
                var reverse = Point3d.Subtract(Position, Curve.PointAt(t));
                reverse.Reverse();
                desiredVelocity = reverse * multiplier;
            }
            return desiredVelocity;
        }
    }

   public class MeshWindForming : IAgentContainment
    {
        public Mesh Mesh { get; set; }
        

        public Vector3d DesiredVector(Point3d Position, Vector3d desiredVelocity)
        {
        var newMesh = Mesh.DuplicateMesh();
        double multiplier = 40;

            if (newMesh.IsPointInside(Position, 0.01, false))
            {
                var reverse = Point3d.Subtract(Position, newMesh.ClosestPoint(Position));
                reverse.Reverse();
                desiredVelocity += reverse * multiplier;
                
                Mesh = MeshOperations.VertexMove(newMesh, Position, 1, desiredVelocity, 0.5, false);
            }
            return desiredVelocity;
        }
    }
}
