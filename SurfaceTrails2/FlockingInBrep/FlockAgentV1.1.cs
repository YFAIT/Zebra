using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingInBrep
{
    public class FlockAgent
    {
        public Point3d Position;
        public Vector3d Velocity;

        public FlockSystem FlockSystem;

        private Vector3d desiredVelocity;

        public FlockAgent(Point3d position, Vector3d velocity)
        {
            Position = position;
            Velocity = velocity;
        }
         
        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97 * Velocity + 0.03 * desiredVelocity;

            if (Velocity.Length > 8.0) Velocity *= 8.0 / Velocity.Length;
            else if (Velocity.Length < 4.0) Velocity *= 4.0 / Velocity.Length;

            Position += Velocity * FlockSystem.Timestep;
        }



        public void ComputeDesiredVelocity(List<FlockAgent> neighbours)
        {
            // First, reset the desired velocity to 0
            desiredVelocity = new Vector3d(0.0, 0.0, 0.0);
            //var bounceMultiplier =30;

            // ===============================================================================
            // Pull the agent back if it gets out of the bounding box 
            // ===============================================================================
            double multiplier = 40;

            if (!FlockSystem.Mesh.IsPointInside(Position, 0.01, false))
            {
                //var reverse = Point3d.Subtract(Position, FlockSystem.BBox.Center);
                //reverse.Reverse();
                var reverse = Point3d.Subtract(Position, FlockSystem.Mesh.ClosestPoint(Position));
                reverse.Reverse();
                desiredVelocity += reverse * multiplier;
            }
            // ===============================================================================
            // If there are no neighbours nearby, the agent will maintain its veloctiy,
            // else it will perform the "alignment", "cohension" and "separation" behaviours
            // ===============================================================================
            if (neighbours.Count == 0)
                desiredVelocity += Velocity; // maintain the current velocity
            else
            {
                // -------------------------------------------------------------------------------
                // "Alignment" behavior 
                // -------------------------------------------------------------------------------
                Vector3d alignment = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
                    alignment += neighbour.Velocity;

                // We divide by the number of neighbours to actually get their average velocity
                alignment /= neighbours.Count;

                desiredVelocity += FlockSystem.AlignmentStrength * alignment;
                // -------------------------------------------------------------------------------
                // "Cohesion" behavior 
                // -------------------------------------------------------------------------------
                Point3d centre = Point3d.Origin;

                foreach (FlockAgent neighbour in neighbours)
                    centre += neighbour.Position;

                // We divide by the number of neighbours to actually get their centre of mass
                centre /= neighbours.Count;

                Vector3d cohesion = centre - Position;

                desiredVelocity += FlockSystem.CohesionStrength * cohesion;
                // -------------------------------------------------------------------------------
                // "Separation" behavior 
                // -------------------------------------------------------------------------------
                Vector3d separation = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
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
                desiredVelocity += FlockSystem.SeparationStrength * separation;
            }
            // ===============================================================================
            // Avoiding the obstacles (repellers)
            // ===============================================================================
            foreach (Circle repeller in FlockSystem.Repellers)
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
            // ===============================================================================
            // Adding attractors
            // ===============================================================================
            foreach (Circle attractor in FlockSystem.Attractors)
            {
                double distanceToAttractor = Position.DistanceTo(attractor.Center);

                Vector3d attraction = attractor.Center - Position;
                // Attraction gets stronger as the agent gets closer to the repeller
                attraction *= (attraction.Length / distanceToAttractor);
                // Attraction strength is also proportional to the radius of the Attractor circle/sphere
                // This allows the user to tweak the Attraction strength by tweaking the radius
                attraction *= 10 * attractor.Radius;


                desiredVelocity += attraction;
            }
            // ===============================================================================
            // Adding Wind
            // ===============================================================================
            desiredVelocity += FlockSystem.Wind * 5;
        }
    }
}
