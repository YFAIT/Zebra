using System.Collections.Generic;
using Rhino.Geometry;
//This interface control rules that any agent should have to be part of a flock
namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public interface IFlockAgent
    {
        Point3d Position { get; set; }
        Vector3d Velocity { get; set; }
        FlockSystem FlockSystem { get; set; }
        void UpdateVelocityAndPosition();
        void ComputeDesiredVelocity(List<IFlockAgent> neighbours);
    }
}