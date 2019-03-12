using System.Collections.Generic;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingInPlane
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

            if (Velocity.Length >8) Velocity *= 8 / Velocity.Length;
            else if (Velocity.Length < 4) Velocity *= 4 / Velocity.Length;

            Position += Velocity * FlockSystem.Timestep;
        }
        public void ComputeDesiredVelocity(List<FlockAgent> neighbours)
        {
            // First, reset the desired velocity to 0
            desiredVelocity = new Vector3d(0.0, 0.0, 0.0);
            // ===============================================================================
            // Pull the agent back if it gets out of the bounding box 
            // ===============================================================================
            double boundingBoxMinX = FlockSystem.Box.PointAt(0, 0, 0).X;
            double boundingBoxMinY = FlockSystem.Box.PointAt(0, 0, 0).Y;
            double boundingBoxMinZ = FlockSystem.Box.PointAt(0, 0, 0).Z;
            double boundingBoxMaxX = FlockSystem.Box.PointAt(1, 1, 1).X;
            double boundingBoxMaxY = FlockSystem.Box.PointAt(1, 1, 1).Y;
            double boundingBoxMaxZ = FlockSystem.Box.PointAt(1, 1, 1).Z;
            double multiplier = 80;

            //if (Position.X < boundingBoxMinX)
            //    desiredVelocity += new Vector3d(boundingBoxMaxX - Position.X, 0, 0) * multiplier;

            //else if (Position.X > boundingBoxMaxX)
            //    desiredVelocity += new Vector3d(-Position.X, 0, 0) * multiplier;

            //if (Position.Y < boundingBoxMinY)
            //    desiredVelocity += new Vector3d(0, boundingBoxMaxY - Position.Y, 0) * multiplier;

            //else if (Position.Y > boundingBoxMaxY)
            //    desiredVelocity += new Vector3d(0, -Position.Y, 0) * multiplier;

            //if (Position.Z < boundingBoxMinZ)
            //    desiredVelocity += new Vector3d(0, 0, boundingBoxMaxZ - Position.Z) * multiplier;

            //else if (Position.Z > boundingBoxMaxZ)
            //    desiredVelocity += new Vector3d(0, 0, -Position.Z) * multiplier;
           
            if (FlockSystem.Curve.Contains(Position) == PointContainment.Outside || FlockSystem.Curve.Contains(Position) == PointContainment.Coincident)
            {
                double t;
                FlockSystem.Curve.ClosestPoint(Position, out t);
                var reverse = Point3d.Subtract(Position, FlockSystem.Curve.PointAt(t));
                reverse.Reverse();
                desiredVelocity = reverse * multiplier;
                //Velocity.Reverse();
                //desiredVelocity.Reverse();
            }

            // ===============================================================================
            // Pull the agent back if it gets in binding curves
            // ===============================================================================
            foreach (Curve curve in FlockSystem.InnerCurves)
            {
                if ( curve.Contains(Position) == PointContainment.Inside || curve.Contains(Position) == PointContainment.Coincident)
                {
                    double innerCurveStrength = 80;
                    double t;
                    FlockSystem.Curve.ClosestPoint(Position, out t);
                    var reverse = Point3d.Subtract(Position, FlockSystem.Curve.PointAt(t));
                    reverse.Reverse();
                    desiredVelocity += reverse * innerCurveStrength;
                }
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
                // ===============================================================================
                // Adding attractors
                // ===============================================================================
                foreach (Circle Attractor in FlockSystem.Attractors)
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
            }
        }
    }
}
