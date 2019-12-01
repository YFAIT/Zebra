using Rhino.Geometry;
//Labels each behaviour according to it's type so we can differenciate between them
public enum BehaviourType
{
    Repeller,
    Attractor,
    AttractorCurve,
    RepellerCurve,
    Wind,
    FollowPoints,
    FollowCurve
}
//Sets the rules and requirement for the the Agent behaviours to get the correct data, While belonging to a certain family or group
namespace SurfaceTrails2.AgentBased.Behaviours
{
    public interface IAgentBehaviours
    {
        BehaviourType BehaviourType { get; set; }
        Point3d Position { get; set; }
        FlockSystem FlockSystem { get; set; }
        Vector3d DesiredVelocity { get; set; }
        Vector3d ComputeDesiredVelocity();
    }
}
