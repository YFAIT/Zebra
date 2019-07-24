using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased
{
    public class GhcFlockingSimulation : GH_Component
    {
        private FlockSystem _flockSystem;

        public GhcFlockingSimulation()
            : base(
                  "Flocking Map To Surface",
                  "FlockingMapToSurface",
                  "Flocking Map To Surface",
                  "YFAtools",
                  "AgentBased")
        {
        }
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
            pManager.AddTextParameter("Info", "I", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "P", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "V", "The agent veloctiies", GH_ParamAccess.list);
            pManager.AddPointParameter("repellers", "r", "repellers", GH_ParamAccess.list);
            pManager.AddPointParameter("testpt", "t", "The agent testpt", GH_ParamAccess.list);
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

            List<FlockAgent.FlockAgent> agents = new List<FlockAgent.FlockAgent>();
            List<double> flockProps = new List<double>();
            var interactions = new List<IAgentBehavioursInteractions>();
            List<IAgentContainment> containments = new List<IAgentContainment>();
            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();
            List<GH_Point> surfacePositions = new List<GH_Point>();
            List<GH_Vector> surfaceVectors = new List<GH_Vector>();
            List<Circle> surfaceRepller = new List<Circle>();
            List<Circle> surfaceAttractors = new List<Circle>();
            //var startPoints = new List<Point3d>();
            //get values from grasshopper
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetDataList("Agents", agents);
            DA.GetDataList("Flock Values", flockProps);
            DA.GetDataList("Interactions", interactions);
            DA.GetDataList("Containment", containments);
            const double xMin = 0;
            const double xMax = 30;
            const double yMin = 0;
            const double yMax = 30;

            var container = (SurfaceContainment)containments[0];

            List<Point3d> points = agents.Select(p => p.Position).ToList();
            BoundingBox box = new BoundingBox(points);


            for (var i = 0; i < agents.Count; i++)
            {
                FlockAgent.FlockAgent agent = agents[i];
                agent.Containment = containments;
                agent.Interactions = interactions;

                foreach (var interaction in interactions)
                {
                    if (interaction.Label == "r")
                    {
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
                    }
                    else if (interaction.Label == "a")
                    {
                        foreach (var attractor in interaction.Circles)
                        {
                            // ===============================================================================================
                            // Remap Attractors
                            // ===============================================================================================
                            double u;
                            double v;
                            container.Surface.ClosestPoint(attractor.Center, out u, out v);

                            var nu = NumberOperations.remap(container.Surface.Domain(0).T0,
                                container.Surface.Domain(0).T1, xMin, xMax, u);
                            var nv = NumberOperations.remap(container.Surface.Domain(1).T0,
                                container.Surface.Domain(1).T1, yMin, yMax, v);
                            Point3d remappedCenter = new Point3d(nu, nv, 0);
                            Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                            surfaceAttractors.Add(remappedCircle);
                        }
                    }
                    else if (interaction.Label == "c")
                        _flockSystem.AttractorCurves = interaction.Curves;
                    else if (interaction.Label == "w")
                        _flockSystem.Wind = interaction.WindVec;
                }
            }

            var ifagents = new List<IFlockAgent>();
            ifagents.AddRange(agents);
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================
            if (iReset || _flockSystem == null)
            {
                _flockSystem = new FlockSystem(/*iCount,surface*/ifagents);
                foreach (var agent in agents)
                {
                    agent.Position = agent.StartPosition;
                }
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
               

                _flockSystem.Repellers = surfaceRepller;
                _flockSystem.Attractors = surfaceAttractors;
                _flockSystem.UseParallel = iUseParallel;
                //flockSystem.AttractorCurves = iAttractorCurves;
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
            //foreach (var flockAgent in _flockSystem.IAgents)
            foreach (var agent in agents)
            {
                //var agent = (FlockAgent.FlockAgent) flockAgent;

                positions.Add(new GH_Point(agent.Position));
                velocities.Add(new GH_Vector(agent.Velocity));
                // ===============================================================================
                // Position on surface 
                // ===============================================================================
                //Interval u = new Interval(0, 1);
                //Interval v = new Interval(0, 1);

                //surface.SetDomain(0, u);
                //surface.SetDomain(1, v);

                //var nu = (agent.Position.X - 0) / (flockSystem.XMax - 0);
                //var nv = (agent.Position.Y - 0) / (flockSystem.XMax - 0);
                    
                var nu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                    container.Surface.Domain(0).T1, agent.Position.X);
                var nv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                    container.Surface.Domain(1).T1, agent.Position.Y);
                

                var vu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                    container.Surface.Domain(0).T1, agent.Velocity.X);
                var vv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                    container.Surface.Domain(1).T1, agent.Velocity.Y);


                foreach (var interaction in agent.Interactions)
                {
                    if (interaction.Label == "r")
                    {
                        foreach (var repeller in interaction.Circles)
                        {
                            // ===============================================================================================
                            // Remap repellers
                            // ===============================================================================================
                            double u;
                            double v;
                            container.Surface.ClosestPoint(repeller.Center, out u, out v);

                            var tu = NumberOperations.remap(xMin, xMax, container.Surface.Domain(0).T0,
                                container.Surface.Domain(0).T1, u);
                            var tv = NumberOperations.remap(yMin, yMax, container.Surface.Domain(1).T0,
                                container.Surface.Domain(1).T1, v);
                            Point3d remappedCenter = new Point3d(tu, tv, 0);
                            Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                            surfaceRepller.Add(remappedCircle);
                        }
                    }
                    else if (interaction.Label == "a")
                    {
                        foreach (var attractor in interaction.Circles)
                        {
                            // ===============================================================================================
                            // Remap Attractors
                            // ===============================================================================================
                            double u;
                            double v;
                            container.Surface.ClosestPoint(attractor.Center, out u, out v);

                            var tu = NumberOperations.remap(container.Surface.Domain(0).T0,
                                container.Surface.Domain(0).T1, xMin, xMax, u);
                            var tv = NumberOperations.remap(container.Surface.Domain(1).T0,
                                container.Surface.Domain(1).T1, yMin, yMax, v);
                            Point3d remappedCenter = new Point3d(tu, tv, 0);
                            Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                            surfaceAttractors.Add(remappedCircle);
                        }
                    }
                }

                surfacePositions.Add(new GH_Point(container.Surface.PointAt(nu, nv)));
                surfaceVectors.Add(new GH_Vector(new Vector3d(container.Surface.PointAt(vu, vv))));
                //surfacePositions.Add(new GH_Point(surface.PointAt(agent.Position.X, agent.Position.Y)));
            }
            DA.SetDataList("Positions", surfacePositions);
            DA.SetDataList("Velocities", surfaceVectors);
            DA.SetDataList("repellers", surfaceRepller);
            DA.SetDataList("testpt", positions);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }
        public override Guid ComponentGuid { get { return new Guid("7066d2ac-3c0a-47db-b22f-4d8c25bf8baa"); } }
    }
}