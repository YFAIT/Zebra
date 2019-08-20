using Rhino.Geometry;

//This Interface control the rules for any containment entity for the flock

namespace SurfaceTrails2.AgentBased.Containment
{
    //Interface for agents to be contained in any geometry it varies based on geometry type
    public interface IAgentContainment
    {
        char Label { get; set; }
        //double Multiplier { get; set; }
        Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity);
    }
}