using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Behaviours;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public class FlockAgent : IFlockAgent

    {
        private Vector3d _desiredVelocity;
        private List<IAgentContainment> _icontainmentList;
        public double MinVelocity { get; set; }
        public double MaxVelocity { get; set; }
        public Point3d Position { get; set; }
        public Point3d StartPosition { get; set; }
        public Vector3d Velocity { get; set; }
        public FlockSystem FlockSystem { get; set; }
        public List<IAgentContainment> Containment
        {
            get { return _icontainmentList; }
            set { _icontainmentList = value; }
        }
        public List<IAgentInteractions> Interactions { get; set; }
        //constructor assigned values
        public FlockAgent(Point3d position, Vector3d velocity) 
         {
        Position = position;
        Velocity = velocity;
         }
        // ===============================================================================
        // Agent behaviours methods Interactions are taken from Flocksystem class 
        // ===============================================================================
        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97 * Velocity + 0.03 * _desiredVelocity;

            if (Velocity.Length > MaxVelocity) Velocity *= MaxVelocity / Velocity.Length;
            else if (Velocity.Length < MinVelocity) Velocity *= MinVelocity / Velocity.Length;

            Position += Velocity * FlockSystem.Timestep;
        }
        public void ComputeDesiredVelocity(List<IFlockAgent> neighbours)
        {
        // First, reset the desired velocity to 0
        _desiredVelocity = new Vector3d(0.0, 0.0, 0.0);
            // ===============================================================================
            // Pull the agent back if it gets out of the bounding box 
            // ===============================================================================
            foreach (var icontainment in _icontainmentList)
            _desiredVelocity += icontainment.DesiredVector(Position, _desiredVelocity);
            // ===============================================================================
            // If there are no neighbours nearby, the agent will maintain its veloctiy,
            // else it will perform the "alignment", "cohension" and "separation" behaviours
            // ===============================================================================
            if (neighbours.Count == 0)
            _desiredVelocity += Velocity; // maintain the current velocity
            else
            {
            // -------------------------------------------------------------------------------
            // "Alignment" behavior 
            // -------------------------------------------------------------------------------
            _desiredVelocity +=AgentBehaviours.Alignment(neighbours, _desiredVelocity, FlockSystem);
            // -------------------------------------------------------------------------------
            // "Cohesion" behavior 
            // -------------------------------------------------------------------------------
            _desiredVelocity += AgentBehaviours.Cohesion(neighbours, Position, _desiredVelocity, FlockSystem);
            // -------------------------------------------------------------------------------
            // "Separation" behavior 
            // -------------------------------------------------------------------------------
            _desiredVelocity += AgentBehaviours.Separation(neighbours, Position, _desiredVelocity, FlockSystem);
            }
            //// ===============================================================================
            //// Interactionbehaviours
            //// ===============================================================================
            foreach (var interaction in Interactions)
            {
                if (interaction.Label == "c")
                {
                    double t;
                    FlockSystem.AttractorCurves[0].ClosestPoint(StartPosition, out t);
                    var curveClosestPoint = FlockSystem.AttractorCurves[0].PointAt(t);
                    interaction.ClosestPoint[0] = curveClosestPoint;
                }
                if (interaction.Label == "a")
                {
                    var closestPoint = PointOperations.ClosestPoints(StartPosition, FlockSystem.Attractors.Select(p => p.Center).ToList(), 1);
                    interaction.ClosestPoint = closestPoint;
                }
                if (interaction.Label == "f")
                {
                    var closestPoint = PointOperations.ClosestPoints(StartPosition, FlockSystem.FollowAttractors.Select(p => p.Center).ToList(), FlockSystem.FollowAttractors.Count);
                    interaction.ClosestPoint = closestPoint;
                }

                interaction.Position = Position;
                interaction.FlockSystem = FlockSystem;
                interaction.DesiredVelocity = _desiredVelocity;
                _desiredVelocity += interaction.ComputeDesiredVelocity();
            }
        }
    }
}
