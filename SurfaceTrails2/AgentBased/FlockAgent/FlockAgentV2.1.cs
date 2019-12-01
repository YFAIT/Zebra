using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Behaviours;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.OperationLibrary;
/*This class manages the behaviour of each agent in the flock, and can stack multiple behaviours using desired velocity
 This class is the dynamic part of code (updates itself with each iteration)
 All elements placed here will calculated with each iteration
 */
namespace SurfaceTrails2.AgentBased.FlockAgent
{
    public class FlockAgent : IFlockAgent
    {
        //Props
        private Vector3d _desiredVelocity;
        public List<IAgentContainment> Containment;
        public double MinVelocity { get; set; }
        public double MaxVelocity { get; set; }
        public Point3d Position { get; set; }
        public Point3d StartPosition { get; set; }
        public Vector3d Velocity { get; set; }
        public FlockSystem FlockSystem { get; set; }
        public BoundingBox SrfBoudningBox { get; set; }
        public GH_Point GHPosition = new GH_Point();
        public GH_Vector GHVelocity = new GH_Vector();
        public List<IAgentBehaviours> Interactions { get; set; }
        //surface params
        private const double XMin = 0;
        private const double XMax = 30;
        private const double YMin = 0;
        private const double YMax = 30;
        //Behaviour params
        private readonly List<Circle> _surfaceRepller = new List<Circle>();
        private readonly List<Circle> _surfaceAttractors = new List<Circle>();
        private SurfaceContainment _container = null;

        private readonly List<Curve> _remappedCurves = new List<Curve>();
        private readonly List<Point3d> _remappedPoints = new List<Point3d>();
        // ===============================================================================================
        // constructor assigned values
        // ===============================================================================================
        public FlockAgent(Point3d position, Vector3d velocity) 
         {
        Position = position;
        Velocity = velocity;
         }
        // Agent behaviours methods Interactions are taken from Flocksystem class 
        // ===============================================================================================
        // method to update velocity and position with each iteration
        // ===============================================================================================
        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97 * Velocity + 0.03 * _desiredVelocity;

            if (Velocity.Length > MaxVelocity) Velocity *= MaxVelocity / Velocity.Length;
            else if (Velocity.Length < MinVelocity) Velocity *= MinVelocity / Velocity.Length;

            Position += Velocity * FlockSystem.Timestep;
        }
        // ===============================================================================================
        // method to add behaviours to agents
        // ===============================================================================================
        public void ComputeDesiredVelocity(List<IFlockAgent> neighbours)
        {
        // First, reset the desired velocity to 0
        _desiredVelocity = new Vector3d(0.0, 0.0, 0.0);
            // Pull the agent back if it gets out of the bounding box 
            foreach (var icontainment in Containment)
            _desiredVelocity += icontainment.DesiredVector(Position, _desiredVelocity);
            // If there are no neighbours nearby, the agent will maintain its veloctiy,
            // else it will perform the "alignment", "cohension" and "separation" behaviours
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
            // -------------------------------------------------------------------------------
            //// Interactionbehaviours
            // -------------------------------------------------------------------------------
            foreach (var interaction in Interactions)
            {
                switch (interaction.BehaviourType)
                {
                    //=======================================================================================
                    //if interaction is repeller
                    case BehaviourType.Repeller:

                        var interactionRepeller = interaction as Repeller;
                        if (Containment[0].Label == 's')
                        {
                            _container = Containment[0] as  SurfaceContainment;

                            foreach (var repeller in interactionRepeller.Circles)
                            {
                                // Remap repellers
                                double u;
                                double v;
                                _container.Surface.ClosestPoint(repeller.Center, out u, out v);

                                var nu = NumberOperations.remap(SrfBoudningBox.Min.X,
                                    SrfBoudningBox.Max.X, XMin, XMax, u);
                                var nv = NumberOperations.remap(SrfBoudningBox.Min.Y,
                                    SrfBoudningBox.Max.Y, YMin, YMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                                _surfaceRepller.Add(remappedCircle);
                            }
                            FlockSystem.Repellers = _surfaceRepller;
                        }
                        else
                            FlockSystem.Repellers = interactionRepeller.Circles;

                        break;
                    //=======================================================================================
                    //if interaction is Attractor
                    case BehaviourType.Attractor:
                        var interactionAttractor = interaction as Attractor;

                        if (Containment[0].Label == 's')
                        {
                            _container = Containment[0] as SurfaceContainment;

                            foreach (var attractor in interactionAttractor.Circles)
                            {
                                // Remap Attractors
                                double u;
                                double v;
                                _container.Surface.ClosestPoint(attractor.Center, out u, out v);

                                var nu = NumberOperations.remap(SrfBoudningBox.Min.X,
                                    SrfBoudningBox.Max.X, XMin, XMax, u);
                                var nv = NumberOperations.remap(SrfBoudningBox.Min.Y,
                                    SrfBoudningBox.Max.Y, YMin, YMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                                _surfaceAttractors.Add(remappedCircle);
                            }
                            FlockSystem.Attractors = _surfaceAttractors;
                        }
                        else
                            FlockSystem.Attractors = interactionAttractor.Circles;

                        var closestPoint = PointOperations.ClosestPoints(Position, FlockSystem.Attractors.Select(p => p.Center).ToList(), 1);
                        interactionAttractor.ClosestPoint = closestPoint[0];
                        break;
                    //=======================================================================================
                    //if interaction is Attractor curve
                    case BehaviourType.AttractorCurve:

                        var interactionAttractorCurve = interaction as AttractorCurve;

                        if (Containment[0].Label == 's')
                        {
                            //getting curve data: points and degree
                            _container = Containment[0] as SurfaceContainment;
                            var attractorcurve = interactionAttractorCurve.Curves[0].ToNurbsCurve();
                            var controlpoints = attractorcurve.Points;
                            var degree = attractorcurve.Degree;

                            foreach (var controlpoint in controlpoints)
                            {
                                double u;
                                double v;
                                _container.Surface.ClosestPoint(controlpoint.Location, out u, out v);

                                var nu = NumberOperations.remap(_container.Surface.Domain(0).T0,
                                    _container.Surface.Domain(0).T1, XMin, XMax, u);
                                var nv = NumberOperations.remap(_container.Surface.Domain(1).T0,
                                    _container.Surface.Domain(1).T1, YMin, YMax, v);
                                Point3d remappedControlPoint = new Point3d(nu, nv, 0);
                                _remappedPoints.Add(remappedControlPoint);
                            }
                            var remappedCurve = Curve.CreateControlPointCurve(_remappedPoints, degree);
                            _remappedCurves.Add(remappedCurve);
                            FlockSystem.AttractorCurves = _remappedCurves;
                        }
                        else
                            FlockSystem.AttractorCurves = interactionAttractorCurve.Curves;


                        FlockSystem.AttractorCurvesSwitch = interactionAttractorCurve.AttractorCurveSwitch;

                        //FlockSystem.AttractorCurves[0].ClosestPoint(StartPosition, out t);
                        //var curveClosestPoint = FlockSystem.AttractorCurves[0].PointAt(t);
                        Point3d curveClosestPoint;

                        if (FlockSystem.AttractorCurvesSwitch)
                        {
                            List<Point3d> curveClosestPoints = new List<Point3d>();
                            foreach (var attractorCurve in FlockSystem.AttractorCurves)
                            {
                                double t;
                                attractorCurve.ClosestPoint(StartPosition, out t);
                                curveClosestPoints.Add(attractorCurve.PointAt(t));
                            }
                             curveClosestPoint = PointOperations.ClosestPoints(StartPosition, curveClosestPoints, 1)[0];
                        }
                        else
                        {
                            List<Point3d> curveClosestPoints = new List<Point3d>();
                            foreach (var attractorCurve in FlockSystem.AttractorCurves)
                            {
                                double t;
                                attractorCurve.ClosestPoint(Position, out t);
                                curveClosestPoints.Add(attractorCurve.PointAt(t));
                            }
                            curveClosestPoint = PointOperations.ClosestPoints(Position, curveClosestPoints, 1)[0];
                        }


                        interactionAttractorCurve.ClosestPoint = curveClosestPoint;
                        break;
                    //=======================================================================================
                    //if interaction is Repeller curve
                    case BehaviourType.RepellerCurve:

                        var interactionRepellerCurve = interaction as RepellerCurve;

                        if (Containment[0].Label == 's')
                        {
                            //getting curve data: points and degree
                            _container = Containment[0] as SurfaceContainment;
                            var attractorcurve = interactionRepellerCurve.Curves[0].ToNurbsCurve();
                            var controlpoints = attractorcurve.Points;
                            var degree = attractorcurve.Degree;

                            foreach (var controlpoint in controlpoints)
                            {
                                double u;
                                double v;
                                _container.Surface.ClosestPoint(controlpoint.Location, out u, out v);

                                var nu = NumberOperations.remap(_container.Surface.Domain(0).T0,
                                    _container.Surface.Domain(0).T1, XMin, XMax, u);
                                var nv = NumberOperations.remap(_container.Surface.Domain(1).T0,
                                    _container.Surface.Domain(1).T1, YMin, YMax, v);
                                Point3d remappedControlPoint = new Point3d(nu, nv, 0);
                                _remappedPoints.Add(remappedControlPoint);
                            }
                            var remappedCurve = Curve.CreateControlPointCurve(_remappedPoints, degree);
                            _remappedCurves.Add(remappedCurve);
                            FlockSystem.RepllerCurves = _remappedCurves;
                        }
                        else
                            FlockSystem.RepllerCurves = interactionRepellerCurve.Curves;

                        //FlockSystem.RepllerCurves[0].ClosestPoint(Position, out t);
                        //var repellerCurveClosestPoint = FlockSystem.RepllerCurves[0].PointAt(t);
                        List<Point3d> repellerCurveClosestPoints = new List<Point3d>();
                        foreach (var repellerCurve in FlockSystem.RepllerCurves)
                        {
                            double t;
                            repellerCurve.ClosestPoint(Position, out t);
                            repellerCurveClosestPoints.Add(repellerCurve.PointAt(t));
                        }
                        var repellerCurveClosestPoint = PointOperations.ClosestPoints(Position, repellerCurveClosestPoints, 1)[0];

                        interactionRepellerCurve.ClosestPoint = repellerCurveClosestPoint;
                        break;
                    //=======================================================================================
                    //if interaction is Follow Organized Points
                    case BehaviourType.FollowPoints:
                        var interactionFollowOrganizedPoints = interaction as FollowOrganizedPoints;
                        FlockSystem.FollowAttractors = interactionFollowOrganizedPoints.Circles;
                        break;
                    //=======================================================================================
                    //if interaction is follow curve
                    case BehaviourType.FollowCurve:
                        var interactionFollowCurve = interaction as FollowCurve;
                        FlockSystem.FollowCurveAttractors = interactionFollowCurve.Circles;
                        break;
                    //=======================================================================================
                    //if interaction is Wind
                    case BehaviourType.Wind:
                        var interactionWind = interaction as Wind;
                        FlockSystem.Wind = interactionWind.WindVec;
                        break;
                }
                interaction.Position = Position;
                interaction.FlockSystem = FlockSystem;
                interaction.DesiredVelocity = _desiredVelocity;
                _desiredVelocity += interaction.ComputeDesiredVelocity();
            }
        }
        // ===============================================================================================
        // method to display output to grasshopper with some optimization in conversion
        // ===============================================================================================
        public void DisplayToGrasshopper()
        {
                if (Containment[0].Label == 's')
                {
                    _container = (SurfaceContainment)Containment[0];

                    var nu = NumberOperations.remap(XMin, XMax, _container.Surface.Domain(0).T0,
                        _container.Surface.Domain(0).T1, Position.X);
                    var nv = NumberOperations.remap(YMin, YMax, _container.Surface.Domain(1).T0,
                        _container.Surface.Domain(1).T1, Position.Y);

                    var vu = NumberOperations.remap(XMin, XMax, _container.Surface.Domain(0).T0,
                        _container.Surface.Domain(0).T1, Velocity.X);
                    var vv = NumberOperations.remap(YMin, YMax, _container.Surface.Domain(1).T0,
                        _container.Surface.Domain(1).T1, Velocity.Y);

                    GHPosition= new GH_Point(_container.Surface.PointAt(nu, nv));
                    GHVelocity= new GH_Vector(new Vector3d(_container.Surface.PointAt(vu, vv)));
                }
                else
                {
                    GHPosition= new GH_Point(Position);
                    GHVelocity= new GH_Vector(Velocity);
                }
        }
    }
}
