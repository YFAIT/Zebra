using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased;

namespace SurfaceTrails2.AgentBased
{
    //behiours in which the agents move
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
        // an object the repells agents away if they get close to it
        public static Vector3d Repellers(Point3d Position, Vector3d desiredVelocity, FlockSystem flockSystem)
        {
            foreach (Circle repeller in flockSystem.Repellers)
            {
                double distanceToRepeller = Position.DistanceTo(repeller.Center);

                Vector3d repulsion = Position - repeller.Center;

                // Repulstion gets stronger as the agent gets closer to the repeller
                repulsion /= (repulsion.Length * distanceToRepeller);

                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                repulsion *= 30.0 * repeller.Radius;
                desiredVelocity += repulsion;
            }
            return desiredVelocity;
        }
        // an object the attracts agents to it 
        public static Vector3d Attractor(Point3d Position, Vector3d desiredVelocity, FlockSystem flockSystem)
        {
            foreach (Circle Attractor in flockSystem.Attractors)
            {
                double distanceToAttractor = Position.DistanceTo(Attractor.Center);

                Vector3d attraction = Attractor.Center - Position;

                // Repulstion gets stronger as the agent gets closer to the repeller
                attraction *= (attraction.Length / distanceToAttractor);

                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                attraction *= 0.1 * Attractor.Radius;

                desiredVelocity += attraction;
            }
            return desiredVelocity;
        }
        //public static Vector3d AttractorCurve(Point3d Position, Vector3d desiredVelocity, FlockSystem flockSystem)
        //{
        //    foreach (Curve AttractorCurve in flockSystem.AttractorCurves)
        //    {
        //        double t;
        //        AttractorCurve.ClosestPoint(Position, out t);
        //        double distanceToAttractor = Position.DistanceTo(AttractorCurve.PointAt(t));
        //        Vector3d attraction = AttractorCurve.PointAt(t) - Position;
        //        // Repulstion gets stronger as the agent gets closer to the repeller
        //        attraction *= (attraction.Length / distanceToAttractor);
        //        // Repulsion strength is also proportional to the radius of the repeller circle/sphere
        //        // This allows the user to tweak the repulsion strength by tweaking the radius
        //        attraction *= 1;
        //        desiredVelocity += attraction;
        //    }
        //    return desiredVelocity;
        //}
        //public static Vector3d AttractorCurve(List<Point3d> Position, Vector3d desiredVelocity, FlockSystem flockSystem)
        //{
        //    int i = 0;
        //    foreach (Curve AttractorCurve in flockSystem.AttractorCurves)
        //    {
        //        double t;
        //        AttractorCurve.ClosestPoint(Position[i], out t);
        //        double distanceToAttractor = Position[i].DistanceTo(AttractorCurve.PointAt(t));
        //        Vector3d attraction = AttractorCurve.PointAt(t) - Position[i];
        //        // Repulstion gets stronger as the agent gets closer to the repeller
        //        attraction *= (attraction.Length / distanceToAttractor);
        //        // Repulsion strength is also proportional to the radius of the repeller circle/sphere
        //        // This allows the user to tweak the repulsion strength by tweaking the radius
        //        attraction *= 5;
        //        desiredVelocity += attraction;
        //        i++;
        //    }
        //    return desiredVelocity;
        //}

        // an Curve the attracts agents to it 
        public static Vector3d AttractorCurve(Point3d closestPoint,Point3d Position, Vector3d desiredVelocity, FlockSystem flockSystem)
        {
            double distanceToAttractor = Position.DistanceTo(closestPoint);
            Vector3d attraction = closestPoint - Position;
            // Repulstion gets stronger as the agent gets closer to the repeller
            attraction *= (attraction.Length / distanceToAttractor);
            // Repulsion strength is also proportional to the radius of the repeller circle/sphere
            // This allows the user to tweak the repulsion strength by tweaking the radius
            attraction *= 10;
            desiredVelocity += attraction;
            return desiredVelocity;
        }
    }
}
public class Alignment : IAgentBehaviours
{
    public List<IFlockAgent> Neighbours { get; set; }
    public Vector3d DesiredVelocity  { get; set; }
    public FlockSystem FlockSystem  { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        Vector3d alignment = Vector3d.Zero;
        foreach (IFlockAgent neighbour in Neighbours)
        alignment += neighbour.Velocity;
        // We divide by the number of neighbours to actually get their average velocity
        alignment /= Neighbours.Count;
        DesiredVelocity += FlockSystem.AlignmentStrength * alignment;
        return DesiredVelocity;
    }
}
public class Cohesion : IAgentBehaviours
{
    public List<IFlockAgent> Neighbours { get; set; }
    public Point3d Position { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        Point3d centre = Point3d.Origin;
        foreach (IFlockAgent neighbour in Neighbours)
            centre += neighbour.Position;
        // We divide by the number of neighbours to actually get their centre of mass
        centre /= Neighbours.Count;
        Vector3d cohesion = centre - Position;
        DesiredVelocity += FlockSystem.CohesionStrength * cohesion;
        return DesiredVelocity;
    }
}

public class Separation : IAgentBehaviours
{
    public List<IFlockAgent> Neighbours { get; set; }
    public Point3d Position { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        Vector3d separation = Vector3d.Zero;
        foreach (IFlockAgent neighbour in Neighbours)
        {
            double distanceToNeighbour = Position.DistanceTo(neighbour.Position);
            if (distanceToNeighbour < FlockSystem.SeparationDistance)
            {
                Vector3d getAway = Position - neighbour.Position;
                /* We scale the getAway vector by inverse of distanceToNeighbour to make 
                   the getAway vector bigger as the agent gets closer to its neighbour */
                separation += getAway / (getAway.Length * distanceToNeighbour);
            }
        }
        DesiredVelocity += FlockSystem.SeparationStrength * separation;
        return DesiredVelocity;
    }
}
public class Repeller : IAgentBehavioursInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d ClosestPoint { get; set; }
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
public class Attractor : IAgentBehavioursInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d ClosestPoint { get; set; }
    public double Multiplier { get; set; }


    public Vector3d ComputeDesiredVelocity()
    {
        foreach (Circle attractor in FlockSystem.Attractors)
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
        return DesiredVelocity;
    }
}
public class AttractorCurve : IAgentBehavioursInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public Point3d ClosestPoint { get; set; }
    public double Multiplier { get; set; }

    public Vector3d ComputeDesiredVelocity()
    {
        double distanceToAttractor = Position.DistanceTo(ClosestPoint);
        Vector3d attraction = ClosestPoint - Position;
        // Repulstion gets stronger as the agent gets closer to the repeller
        attraction *= (attraction.Length / distanceToAttractor);
        // Repulsion strength is also proportional to the radius of the repeller circle/sphere
        // This allows the user to tweak the repulsion strength by tweaking the radius
        attraction *= Multiplier;
        DesiredVelocity += attraction;
        return DesiredVelocity;
    }
}
public class Wind : IAgentBehavioursInteractions
{
    public string Label { get; set; }
    public List<Curve> Curves { get; set; }
    public List<Circle> Circles { get; set; }
    public Point3d Position { get; set; }
    public Vector3d WindVec { get; set; }
    public Vector3d DesiredVelocity { get; set; }
    public FlockSystem FlockSystem { get; set; }
    public Point3d ClosestPoint { get; set; }
    public double Multiplier { get; set; }
    public Vector3d ComputeDesiredVelocity()
    {
        DesiredVelocity += FlockSystem.Wind * Multiplier;
        return DesiredVelocity;
    }
}
