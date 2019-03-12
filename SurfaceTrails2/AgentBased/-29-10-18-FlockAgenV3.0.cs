 using System.Collections.Generic;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased;
using SurfaceTrails2.AgentBased.FlockingInBox;

namespace SurfaceTrails2.FlockingInBox
{
    public class FlockAgent : IFlockAgent

    {
        private Vector3d desiredVelocity;
        private IAgentContainment Icontainment;
        private IAgentContainment Icontainment2;

        private List<IAgentBehaviour> _behaviours = new List<IAgentBehaviour>();

        public double MinVelocity { get; set; }
        public double MaxVelocity { get; set; }
        public Point3d Position { get; set; }
        public Point3d ClosestPoint { get; set; }
        public Vector3d Velocity { get; set; }
        public FlockSystem FlockSystem { get; set; }

        public IAgentContainment IContainment
        {
            get { return Icontainment; }
            set { Icontainment = value; }
        }

        public IAgentContainment IContainment2
        {
            get { return Icontainment2; }
            set { Icontainment2 = value; }
        }

        public List<IAgentBehaviour> Behaviours
        {
            get { return _behaviours; }
            set { _behaviours = value; }
        }

        public FlockAgent(Point3d position, Vector3d velocity)

        {
            Position = position;
            Velocity = velocity;
        }

        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97 * Velocity + 0.03 * desiredVelocity;

            if (Velocity.Length > MaxVelocity) Velocity *= MaxVelocity / Velocity.Length;
            else if (Velocity.Length < MinVelocity) Velocity *= MinVelocity / Velocity.Length;

            Position += Velocity * FlockSystem.Timestep;
        }

        public void ComputeDesiredVelocity(List<IFlockAgent> neighbours)
        {
            // First, reset the desired velocity to 0
            desiredVelocity = new Vector3d(0.0, 0.0, 0.0);
            // ===============================================================================
            // Pull the agent back if it gets out of the bounding box 
            // ===============================================================================
            desiredVelocity += Icontainment.DesiredVector(Position, desiredVelocity);
            desiredVelocity += Icontainment2.DesiredVector(Position, desiredVelocity);
            // ===============================================================================
            // If there are no neighbours nearby, the agent will maintain its veloctiy,
            // else it will perform the "alignment", "cohension" and "separation" behaviours
            // ===============================================================================
            if (neighbours.Count == 0)
                desiredVelocity += Velocity; // maintain the current velocity
            else
            {
                // -------------------------------------------------------------------------------
                //  behavior 
                // -------------------------------------------------------------------------------
                foreach (IAgentBehaviour behaviour in _behaviours)
                {
                    desiredVelocity += behaviour.Behaviour(neighbours, Position, desiredVelocity, FlockSystem);
                }
            }
        }
    }
}
