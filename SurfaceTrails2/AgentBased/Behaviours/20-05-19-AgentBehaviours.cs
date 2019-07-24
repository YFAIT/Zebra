using System.Collections.Generic;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased;
using SurfaceTrails2.AgentBased.Behaviours;

namespace SurfaceTrails2.AgentBased
{
    // ===============================================================================================
    // Agent main behaviours (cohesion, alignment, seperation)
    // ===============================================================================================
    static class AgentBehaviours
    {
        //agent move in the same direction of the neighbours
        public static Vector3d Alignment(List<IFlockAgent> neighbours, Vector3d desiredVelocity,
            FlockSystem flockSystem)
        {
            Vector3d alignment = Vector3d.Zero;

            foreach (IFlockAgent neighbour in neighbours)
                alignment += neighbour.Velocity;

            // We divide by the number of neighbours to actually get their average velocity
            alignment /= neighbours.Count;

            desiredVelocity += flockSystem.AlignmentStrength * alignment;
            return desiredVelocity;
        }
        //agent move towards the center of the neighbours
        public static Vector3d Cohesion(List<IFlockAgent> neighbours, Point3d Position, Vector3d desiredVelocity,
            FlockSystem flockSystem)
        {
            Point3d centre = Point3d.Origin;

            foreach (IFlockAgent neighbour in neighbours)
                centre += neighbour.Position;

            // We divide by the number of neighbours to actually get their centre of mass
            centre /= neighbours.Count;

            Vector3d cohesion = centre - Position;

            desiredVelocity += flockSystem.CohesionStrength * cohesion;
            return desiredVelocity;
        }
        //agent move away from neighbours if it gets too close
        public static Vector3d Separation(List<IFlockAgent> neighbours, Point3d Position, Vector3d desiredVelocity,
            FlockSystem flockSystem)
        {
            Vector3d separation = Vector3d.Zero;

            foreach (IFlockAgent neighbour in neighbours)
            {
                double distanceToNeighbour = Position.DistanceTo(neighbour.Position);

                if (distanceToNeighbour < flockSystem.SeparationDistance)
                {
                    Vector3d getAway = Position - neighbour.Position;

                    /* We scale the getAway vector by inverse of distanceToNeighbour to make 
                       the getAway vector bigger as the agent gets closer to its neighbour */
                    separation += getAway / (getAway.Length * distanceToNeighbour);
                }
            }
            desiredVelocity += flockSystem.SeparationStrength * separation;
            return desiredVelocity;
        }
    }
}
// ===============================================================================================
// Agent advanced behaviours (attractor, repluser, ...etc)
// ===============================================================================================
public class Repeller : IAgentInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Circle> Circles { get; set; }
    public List<Point3d> ClosestPoint { get; set; }
    public double Multiplier { get; set; }

    public Vector3d ComputeDesiredVelocity()
    {
        foreach (Circle repeller in FlockSystem.Repellers)
        {
            double distanceToRepeller = Position.DistanceTo(repeller.Center);
            Vector3d repulsion = Position - repeller.Center;
            // Repulstion gets stronger as the agent gets closer to the repeller
            repulsion /= (repulsion.Length * distanceToRepeller);
            // Repulsion strength is also proportional to the radius of the repeller circle/sphere
            // This allows the user to tweak the repulsion strength by tweaking the radius
            repulsion *= Multiplier * repeller.Radius;
            DesiredVelocity += repulsion;
        }
        return DesiredVelocity;
    }
}
public class Attractor : IAgentInteractions
{
    public bool AttractToNearestPt { get; set; }
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Circle> Circles { get; set; }
    public List<Point3d> ClosestPoint { get; set; }
    public double Multiplier { get; set; }


    public Vector3d ComputeDesiredVelocity()
    {
        foreach (Circle attractor in FlockSystem.Attractors)
        {
           
            if (AttractToNearestPt)
            {
                double distanceToAttractor = Position.DistanceTo(ClosestPoint[0]);
                Vector3d attraction = ClosestPoint[0] - Position;
                // Repulstion gets stronger as the agent gets closer to the repeller
                attraction *= (attraction.Length / distanceToAttractor);
                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                attraction *= Multiplier * attractor.Radius;
                DesiredVelocity += attraction;
            }
            else
            {
                double distanceToAttractor = Position.DistanceTo(attractor.Center);
                Vector3d attraction = attractor.Center - Position;
                // Repulstion gets stronger as the agent gets closer to the repeller
                attraction *= (attraction.Length / distanceToAttractor);
                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                attraction *= Multiplier * attractor.Radius;
                DesiredVelocity += attraction;
            }
        }
        return DesiredVelocity;
    }
}
public class AttractorCurve : IAgentInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Point3d> ClosestPoint { get; set; }
    public double Multiplier { get; set; }

    public Vector3d ComputeDesiredVelocity()
    {
        double distanceToAttractor = Position.DistanceTo(ClosestPoint[0]);
        Vector3d attraction = ClosestPoint[0] - Position;
        // Repulstion gets stronger as the agent gets closer to the repeller
        attraction *= (attraction.Length / distanceToAttractor);
        // Repulsion strength is also proportional to the radius of the repeller circle/sphere
        // This allows the user to tweak the repulsion strength by tweaking the radius
        attraction *= Multiplier;
        DesiredVelocity += attraction;
        return DesiredVelocity;
    }
}
public class Wind : IAgentInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Point3d> ClosestPoint { get; set; }
    public double Multiplier { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        DesiredVelocity += FlockSystem.Wind * Multiplier;
        return DesiredVelocity;
    }
}

public class FollowOrganizedPoints : IAgentInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Point3d> ClosestPoint { get; set; }
    public double Multiplier { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        for (var i = 0; i < FlockSystem.FollowAttractors.Count; i++)
        {
            Circle attractor = FlockSystem.FollowAttractors[i];
            double distanceToAttractor = Position.DistanceTo(ClosestPoint[0]);
            Vector3d attraction = ClosestPoint[0] - Position;
            // Repulstion gets stronger as the agent gets closer to the repeller
            attraction *= (attraction.Length / distanceToAttractor);
            // Repulsion strength is also proportional to the radius of the repeller circle/sphere
            // This allows the user to tweak the repulsion strength by tweaking the radius
            attraction *= Multiplier * attractor.Radius;
            DesiredVelocity += attraction;
            if (ClosestPoint[0].DistanceTo(Position) < ClosestPoint[0].DistanceTo(ClosestPoint[1])/2)
            {
                attraction = ClosestPoint[1] - Position;
                // Repulstion gets stronger as the agent gets closer to the repeller
                attraction *= (attraction.Length / distanceToAttractor);
                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                attraction *= Multiplier * attractor.Radius;
                DesiredVelocity += attraction;
            }
        }
        return DesiredVelocity;
    }
}
