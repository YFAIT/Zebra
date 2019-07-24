using System.Collections.Generic;
using Rhino.Geometry;

namespace SurfaceTrails2.AgentBased.Behaviours
{
    public interface IAgentInteractions
    {
        string Label { get; set; }
        List<Point3d> ClosestPoint { get; set; }
        List<Curve> Curves { get; set; }
        List<Circle> Circles { get; set; }
        Point3d Position { get; set; }
        Vector3d WindVec { get; set; }
        FlockSystem FlockSystem { get; set; }
        Vector3d DesiredVelocity { get; set; }
        Vector3d ComputeDesiredVelocity();
    }
}
