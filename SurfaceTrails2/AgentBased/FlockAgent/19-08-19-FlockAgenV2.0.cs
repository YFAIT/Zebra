using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Behaviours;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.OperationLibrary;

//This class manages the behaviour of each agent in the flock, and can stack multiple behaviours using desired velocity
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

        public BoundingBox SrfBoudningBox { get; set; }

       public GH_Point GHPosition = new GH_Point();
       public GH_Vector GHVelocities = new GH_Vector();

        public List<IAgentContainment> Containment
        {
            get { return _icontainmentList; }
            set { _icontainmentList = value; }
        }
        public List<IAgentBehaviours> Interactions { get; set; }

        //surface params
        const double xMin = 0;
        const double xMax = 30;
        const double yMin = 0;
        const double yMax = 30;

        List<Circle> surfaceRepller = new List<Circle>();
        List<Circle> surfaceAttractors = new List<Circle>();
        SurfaceContainment container = null;

        List<Curve> remappedCurves = new List<Curve>();
        List<Point3d> remappedPoints = new List<Point3d>();
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
                switch (interaction.BehaviourType)
                {
                    //=======================================================================================
                    //if interaction is repeller
                    case BehaviourType.Repeller:
                        if (_icontainmentList[0].Label == 's')
                        {
                            container = (SurfaceContainment)_icontainmentList[0];

                            foreach (var repeller in interaction.Circles)
                            {
                                // ===============================================================================================
                                // Remap repellers
                                // ===============================================================================================
                                double u;
                                double v;
                                container.Surface.ClosestPoint(repeller.Center, out u, out v);

                                var nu = NumberOperations.remap(SrfBoudningBox.Min.X,
                                    SrfBoudningBox.Max.X, xMin, xMax, u);
                                var nv = NumberOperations.remap(SrfBoudningBox.Min.Y,
                                    SrfBoudningBox.Max.Y, yMin, yMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                                surfaceRepller.Add(remappedCircle);
                            }
                            FlockSystem.Repellers = surfaceRepller;
                        }
                        else
                            FlockSystem.Repellers = interaction.Circles;

                        break;
                    case BehaviourType.Attractor:
                        if (_icontainmentList[0].Label == 's')
                        {
                            container = (SurfaceContainment)_icontainmentList[0];

                            foreach (var attractor in interaction.Circles)
                            {
                                // ===============================================================================================
                                // Remap Attractors
                                // ===============================================================================================
                                double u;
                                double v;
                                container.Surface.ClosestPoint(attractor.Center, out u, out v);

                                var nu = NumberOperations.remap(SrfBoudningBox.Min.X,
                                    SrfBoudningBox.Max.X, xMin, xMax, u);
                                var nv = NumberOperations.remap(SrfBoudningBox.Min.Y,
                                    SrfBoudningBox.Max.Y, yMin, yMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                                surfaceAttractors.Add(remappedCircle);
                            }
                            FlockSystem.Attractors = surfaceAttractors;
                        }
                        else
                            FlockSystem.Attractors = interaction.Circles;

                        var closestPoint = PointOperations.ClosestPoints(Position, FlockSystem.Attractors.Select(p => p.Center).ToList(), 1);
                        interaction.ClosestPoint = closestPoint[0];
                        break;

                    case BehaviourType.AttractorCurve:

                        if (_icontainmentList[0].Label == 's')
                        {
                            //getting curve data: points and degree
                            container = (SurfaceContainment)_icontainmentList[0];
                            var interactionAttractorCurve = (AttractorCurve)interaction;
                            var attractorcurve = interactionAttractorCurve.Curves[0].ToNurbsCurve();
                            var controlpoints = attractorcurve.Points;
                            var degree = attractorcurve.Degree;

                            foreach (var controlpoint in controlpoints)
                            {
                                double u;
                                double v;
                                container.Surface.ClosestPoint(controlpoint.Location, out u, out v);

                                var nu = NumberOperations.remap(container.Surface.Domain(0).T0,
                                    container.Surface.Domain(0).T1, xMin, xMax, u);
                                var nv = NumberOperations.remap(container.Surface.Domain(1).T0,
                                    container.Surface.Domain(1).T1, yMin, yMax, v);
                                Point3d remappedControlPoint = new Point3d(nu, nv, 0);
                                remappedPoints.Add(remappedControlPoint);
                            }
                            var remappedCurve = Curve.CreateControlPointCurve(remappedPoints, degree);
                            remappedCurves.Add(remappedCurve);
                            FlockSystem.AttractorCurves = remappedCurves;
                        }
                        else
                            FlockSystem.AttractorCurves = interaction.Curves;

                        double t;
                        FlockSystem.AttractorCurves[0].ClosestPoint(StartPosition, out t);
                        var curveClosestPoint = FlockSystem.AttractorCurves[0].PointAt(t);
                        interaction.ClosestPoint = curveClosestPoint;
                        break;
                    case BehaviourType.FollowPoints:
                        FlockSystem.FollowAttractors = interaction.Circles;
                        break;
                    case BehaviourType.FollowCurve:
                        FlockSystem.FollowCurveAttractors = interaction.Circles;
                        break;
                    case BehaviourType.Wind:
                        FlockSystem.Wind = interaction.WindVec;
                        break;
                }
                interaction.Position = Position;
                interaction.FlockSystem = FlockSystem;
                interaction.DesiredVelocity = _desiredVelocity;
                _desiredVelocity += interaction.ComputeDesiredVelocity();
            }
        }

        public void DisplayToGrasshopper()
        {
                if (_icontainmentList[0].Label == 's')
                {
                    container = (SurfaceContainment)_icontainmentList[0];

                    var nu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                        container.Surface.Domain(0).T1, Position.X);
                    var nv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                        container.Surface.Domain(1).T1, Position.Y);

                    var vu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                        container.Surface.Domain(0).T1, Velocity.X);
                    var vv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                        container.Surface.Domain(1).T1, Velocity.Y);

                    GHPosition= new GH_Point(container.Surface.PointAt(nu, nv));
                    GHVelocities= new GH_Vector(new Vector3d(container.Surface.PointAt(vu, vv)));
                }
                else
                {
                    GHPosition= new GH_Point(Position);
                    GHVelocities= new GH_Vector(Velocity);
                }
        }
    }
}
