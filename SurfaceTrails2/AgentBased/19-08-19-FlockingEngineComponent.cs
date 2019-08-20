using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Behaviours;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.Utilities;
/*This Class is the main grasshopper component fo flocking that gets supplied by options to change the flocking attributes
 *Also this class controls the timing for the whole flocking algorithm
 */

namespace SurfaceTrails2.AgentBased
{
    public class GhcFlockingInBox : GH_Component
    {
        private FlockSystem _flockSystem;

        public GhcFlockingInBox()
            : base(
                  "Flocking Engine",
                  "Engine",
                  "Flocking Engine",
                  "YFAtools",
                  "AgentBased")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Use Parallel", "P", "Use Parallel", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Use R-Tree", "T", "Use R-Tree", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "P", "Play", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Agents", "A", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddNumberParameter("Flock Values", "F", "Flock Values", GH_ParamAccess.list);
            pManager.AddGenericParameter("Interactions", "I", "Interactions", GH_ParamAccess.list);
            pManager[6].Optional = true;
            pManager.AddGenericParameter("Containment", "C", "Containment", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "P", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "V", "The agent veloctiies", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
// ===============================================================================================
// Read input parameters
// ===============================================================================================
            bool iUseParallel = false;
            bool iUseRTree = false;
            bool iReset = true;
            bool iPlay = false;
            List<double> flockProps = new List<double>();
            List<IAgentBehaviours> interactions = new List<IAgentBehaviours>();
            List<FlockAgent.FlockAgent> agents = new List<FlockAgent.FlockAgent>();
            List<GH_Point> surfacePositions = new List<GH_Point>();
            List<IAgentContainment> containments = new List<IAgentContainment>();
//get values from grasshopper
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetDataList("Agents", agents);
            DA.GetDataList("Flock Values", flockProps);
            DA.GetDataList("Interactions", interactions);
            DA.GetDataList("Containment", containments);
//surface params
            const double xMin = 0;
            const double xMax = 30;
            const double yMin = 0;
            const double yMax = 30;

            var points = agents.Select(p => p.Position).ToList();
            BoundingBox box = new BoundingBox(points);
            List<Circle> surfaceRepller = new List<Circle>();
            List<Circle> surfaceAttractors = new List<Circle>();
            List<GH_Vector> surfaceVectors = new List<GH_Vector>();
            SurfaceContainment container = null;

            var remappedCurves = new List<Curve>();
            var remappedPoints = new List<Point3d>();
//assigning values to flock agents
            foreach (FlockAgent.FlockAgent agent in agents)
            {
                agent.Containment = containments;
                agent.Interactions = interactions;
            }
            var ifagents = new List<IFlockAgent>();
            ifagents.AddRange(agents);
// ===============================================================================================
// Read input parameters
// ===============================================================================================
            if (iReset || _flockSystem == null)
            {
                _flockSystem = new FlockSystem(ifagents);
                foreach (var agent in agents)
                    agent.Position = agent.StartPosition;
            }
            else
            {
// ===============================================================================================
// Assign the input parameters to the corresponding variables in the  "flockSystem" object
// ===============================================================================================
                 _flockSystem.Timestep = flockProps[1];
                 _flockSystem.NeighbourhoodRadius = flockProps[2];
                 _flockSystem.AlignmentStrength = flockProps[3];
                 _flockSystem.CohesionStrength = flockProps[4];
                 _flockSystem.SeparationStrength = flockProps[5];
                 _flockSystem.SeparationDistance = flockProps[6];


                // ===============================================================================================
                // ===============================================================================================
                // Code Needs enhancement regarding surface to remove all core code from the main code starting from here to the end of code.
                // ===============================================================================================
                // ===============================================================================================
                foreach (var interaction in interactions)
                {
                    if (interaction.Label == "r")
                    {
                        if (containments[0].Label == "s")
                        {
                            container = (SurfaceContainment)containments[0];

                            foreach (var repeller in interaction.Circles)
                            {
// ===============================================================================================
// Remap repellers
// ===============================================================================================
                                double u;
                                double v;
                                container.Surface.ClosestPoint(repeller.Center, out u, out v);

                                var nu = NumberOperations.remap(box.Min.X,
                                    box.Max.X, xMin, xMax, u);
                                var nv = NumberOperations.remap(box.Min.Y,
                                    box.Max.Y, yMin, yMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                                surfaceRepller.Add(remappedCircle);
                            }
                            _flockSystem.Repellers = surfaceRepller;

                        }

                        else
                        _flockSystem.Repellers = interaction.Circles;
                    }
                    else if (interaction.Label == "a")
                    {
                        if (containments[0].Label == "s")
                        {
                            container = (SurfaceContainment)containments[0];

                            foreach (var attractor in interaction.Circles)
                            {
// ===============================================================================================
// Remap Attractors
// ===============================================================================================
                                double u;
                                double v;
                                container.Surface.ClosestPoint(attractor.Center, out u, out v);

                                var nu = NumberOperations.remap(box.Min.X,
                                    box.Max.X, xMin, xMax, u);
                                var nv = NumberOperations.remap(box.Min.Y,
                                    box.Max.Y, yMin, yMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                                surfaceAttractors.Add(remappedCircle);
                            }
                            _flockSystem.Attractors = surfaceAttractors;
                        }
                        else
                        _flockSystem.Attractors = interaction.Circles;
                    }
                    else if (interaction.Label == "c")
                    {
                        if (containments[0].Label == "s")
                        {
 //getting curve data: points and degree
                            container = (SurfaceContainment)containments[0];
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
                            _flockSystem.AttractorCurves = remappedCurves;
                        }

                        else
                        _flockSystem.AttractorCurves = interaction.Curves;
                    }
                
                }

                _flockSystem.UseParallel = iUseParallel;
// ===============================================================================
// Update the flock
// ===============================================================================
            if (iUseRTree)
                _flockSystem.UpdateUsingRTree();
            else
                _flockSystem.Update();

            if (iPlay) ExpireSolution(true);
            }
// ===============================================================================
// Output the agent positions and velocities so we can see them on display
// ===============================================================================
            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();

            foreach (var flockAgent in _flockSystem.IAgents)
            {
                var agent = (FlockAgent.FlockAgent)flockAgent;
              
                positions.Add(new GH_Point(agent.Position));
                velocities.Add(new GH_Vector(agent.Velocity));

                if (containments[0].Label == "s")
                {
                    container = (SurfaceContainment)containments[0];

                    var nu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                        container.Surface.Domain(0).T1, agent.Position.X);
                    var nv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                        container.Surface.Domain(1).T1, agent.Position.Y);


                    var vu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                        container.Surface.Domain(0).T1, agent.Velocity.X);
                    var vv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                        container.Surface.Domain(1).T1, agent.Velocity.Y);


                    surfacePositions.Add(new GH_Point(container.Surface.PointAt(nu, nv)));
                    surfaceVectors.Add(new GH_Vector(new Vector3d(container.Surface.PointAt(vu, vv))));
                }
            }
//Export data to grasshopper
            if (containments[0].Label == "s")
            {
                DA.SetDataList("Positions", surfacePositions);
                DA.SetDataList("Velocities", surfaceVectors);
            }
            else
            {
                DA.SetDataList("Positions", positions);
                DA.SetDataList("Velocities", velocities);
            }
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Engine__2_; } }
        public override Guid ComponentGuid { get { return new Guid("d7f22bc3-fc53-4ad9-ae67-6597d259d671"); } }
    }
}