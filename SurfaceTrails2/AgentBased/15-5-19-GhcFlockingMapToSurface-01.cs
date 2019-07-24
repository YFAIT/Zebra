using System;
using System.Collections.Generic;
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
            pManager.AddBooleanParameter("Use Parallel", "Use Parallel", "Use Parallel", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Use R-Tree", "Use R-Tree", "Use R-Tree", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "Play", "Play", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddNumberParameter("Flock Values", "F", "Flock Values", GH_ParamAccess.list);
            pManager.AddGenericParameter("Interactions", "I", "Interactions", GH_ParamAccess.list);
            pManager[6].Optional = true;
            pManager.AddGenericParameter("Containment", "C", "Containment", GH_ParamAccess.list);



            //pManager.AddIntegerParameter("Count", "Count", "Number of Agents", GH_ParamAccess.item, 50);
            //pManager.AddNumberParameter("Timestep", "Timestep", "Timestep", GH_ParamAccess.item, 0.02);
            //pManager.AddNumberParameter("Neighbourhood Radius", "Neighbourhood Radius", "Neighbourhood Radius", GH_ParamAccess.item, 3.5);
            //pManager.AddNumberParameter("Alignment", "Alignment", "Alignment", GH_ParamAccess.item, 0.5);
            //pManager.AddNumberParameter("Cohesion", "Cohesion", "Cohesion", GH_ParamAccess.item, 0.5);
            //pManager.AddNumberParameter("Separation", "Separation", "Separation", GH_ParamAccess.item, 0.5);
            //pManager.AddNumberParameter("Separation Distance", "Separation Distance", "Separation Distance", GH_ParamAccess.item, 1.5);
            //pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            //pManager[7].Optional = true;
            //pManager.AddCircleParameter("Attractors", "Attractors", "Attractors", GH_ParamAccess.list);
            //pManager[8].Optional = true;
            //pManager.AddCurveParameter("AttractorCurves", "AttractorCurves", "AttractorCurves", GH_ParamAccess.list);
            //pManager[13].Optional = true;
            //pManager.AddPointParameter("AttractorCurvePoints", "AttractorCurvePoints", "AttractorCurvePoints", GH_ParamAccess.list);
            //pManager[14].Optional = true;
            //pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            //pManager[15].Optional = true;
            //pManager.AddSurfaceParameter("srf", "srf", "srf", GH_ParamAccess.item);

        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "Positions", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent veloctiies", GH_ParamAccess.list);
            pManager.AddPointParameter("repellers", "repellers", "repellers", GH_ParamAccess.list);
            pManager.AddPointParameter("testpt", "testpt", "The agent testpt", GH_ParamAccess.list);

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

            //int iCount = 0;
            //double iTimestep = 0.0;
            //double iNeighbourhoodRadius = 0.0;
            //double iAlignment = 0.0;
            //double iCohesion = 0.0;
            //double iSeparation = 0.0;
            //double iSeparationDistance = 0.0;
            List<Circle> iRepellers = new List<Circle>();
            List<Circle> iAttractors = new List<Circle>();
            //List<Curve> iAttractorCurves = new List<Curve>();
            Vector3d wind = Vector3d.Unset;
            //Surface baseSurface = null;
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

            //DA.GetData("Count", ref iCount);
            //DA.GetData("Timestep", ref iTimestep);
            //DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius);
            //DA.GetData("Alignment", ref iAlignment);
            //DA.GetData("Cohesion", ref iCohesion);
            //DA.GetData("Separation", ref iSeparation);
            //DA.GetData("Separation Distance", ref iSeparationDistance);
            //DA.GetDataList("Repellers", iRepellers);
            //DA.GetDataList("Attractors", iAttractors);
            //DA.GetDataList("AttractorCurves", iAttractorCurves);
            //DA.GetDataList("AttractorCurvePoints", startPoints);
            //DA.GetData("Wind", ref wind);
            //DA.GetData("srf", ref baseSurface);
            //assign values to agents
            //var surface = baseSurface.ToNurbsSurface();
            //var boundingBox = surface.GetBoundingBox(true);
            //var xMin = boundingBox.Corner(true, true, true).X;
            //var xMax = boundingBox.Corner(false, true, true).X;
            //var yMin = boundingBox.Corner(true, true, true).Y;
            //var yMax = boundingBox.Corner(true, false, true).Y;
            const double xMin = 0;
            const double xMax = 30;
            const double yMin = 0;
            const double yMax = 30;

            //for (int i = 0; i < iCount; i++)
            //{
            //    BoxAgent agent = new BoxAgent(
            //        Util.GetRandomPoint(xMin, xMax, yMin, yMax, 0.0, 0.0),
            //        Util.GetRandomUnitVectorXY() * 4.0);

            //    agents.Add(agent);
            //}

            //Wind windb = new Wind();
            //windb.WindVec = wind;


            //Attractor atttractor = new Attractor();
            //atttractor.Circles = iAttractors;
            //atttractor.Multiplier = 5;

            //interactions.Add(windb);
            //interactions.Add(atttractor);

            //var closestPoints = new List<Point3d>();
            //closest Points to startpoints
            //----------------------------------------------------------------------------------------
            //for (int i = 0; i < startPoints.Count; i++)
            //{
            //    double t;
            //    iAttractorCurves[0].ClosestPoint(startPoints[i], out t);
            //    var curveClosestPoint = iAttractorCurves[0].PointAt(t);

            //    double u2;
            //    double v2;
            //    surface.ClosestPoint(curveClosestPoint, out u2, out v2);
            //    var nu2 = NumberOperations.remap(surface.Domain(0).T0,
            //        surface.Domain(0).T1, xMin, xMax, u2);
            //    var nv2 = NumberOperations.remap(surface.Domain(1).T0,
            //        surface.Domain(1).T1, yMin, yMax, v2);

            //    closestPoints.Add(new Point3d(nu2, nv2, 0));



            //    double u;
            //    double v;
            //    surface.ClosestPoint(startPoints[i], out u, out v);
            //    var nu = NumberOperations.remap(surface.Domain(0).T0,
            //        surface.Domain(0).T1, xMin, xMax, u);
            //    var nv = NumberOperations.remap(surface.Domain(1).T0,
            //        surface.Domain(1).T1, yMin, yMax, v);

            //    agents[i].Position = new Point3d(nu, nv, 0);
            //}

            //int j = 0;

            //var surfaceContainment = new SurfaceContainment
            //{
            //    Surface = surface,
            //    //xMin = xMin,
            //    //xMax = xMax,
            //    //yMin = yMin,
            //    //yMax = yMax,


            //    xMin = surface.Domain(0).T0,
            //    xMax = surface.Domain(0).T1,
            //    yMin = surface.Domain(1).T0,
            //    yMax = surface.Domain(1).T1,
            //    Multiplier = 1.0,
            //    Label = "s"
            //};
            //containments.Add(surfaceContainment);

            foreach (FlockAgent.FlockAgent agent in agents)
            {
                //double u;
                //double v;
                //surface.ClosestPoint(agent.Position, out u, out v);
                //var nu = NumberOperations.remap(surface.Domain(0).T0,
                //    surface.Domain(0).T1, xMin, xMax, u);
                //var nv = NumberOperations.remap(surface.Domain(1).T0,
                //    surface.Domain(1).T1, yMin, yMax, v);

                //agent.Position = new Point3d(nu, nv, 0);

                //agent.Position = new Point3d(agent.Position.X, agent.Position.Y, 0);
              

                //agent.Containment = surfaceContainment;
                //agent.Containment.Add(surfaceContainment);
                agent.Containment = containments;

                agent.Interactions = interactions;

                //agent.ClosestPoint = closestPoints[j];
                //j++;
            }
            var ifagents = new List<IFlockAgent>();
            ifagents.AddRange(agents);

            var container = (SurfaceContainment)containments[0];

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
                //flockSystem.Timestep = iTimestep;
                //flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                //flockSystem.AlignmentStrength = iAlignment;
                //flockSystem.CohesionStrength = iCohesion;
                //flockSystem.SeparationStrength = iSeparation;
                //flockSystem.SeparationDistance = iSeparationDistance;
                _flockSystem.Timestep = flockProps[1];
                _flockSystem.NeighbourhoodRadius = flockProps[2];
                _flockSystem.AlignmentStrength = flockProps[3];
                _flockSystem.CohesionStrength = flockProps[4];
                _flockSystem.SeparationStrength = flockProps[5];
                _flockSystem.SeparationDistance = flockProps[6];

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

                                var nu = NumberOperations.remap(container.Surface.Domain(0).T0,
                                    container.Surface.Domain(0).T1, xMin, xMax, u);
                                var nv = NumberOperations.remap(container.Surface.Domain(1).T0,
                                    container.Surface.Domain(1).T1, yMin, yMax, v);
                                Point3d remappedCenter = new Point3d(nu, nv, 0);
                                Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                                surfaceRepller.Add(remappedCircle);
                                _flockSystem.Repellers = surfaceRepller;
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

                _flockSystem.Attractors = surfaceAttractors;
                _flockSystem.UseParallel = iUseParallel;
                //flockSystem.AttractorCurves = iAttractorCurves;
                _flockSystem.Wind = wind;
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
            foreach (var flockAgent in _flockSystem.IAgents)
            {
                var agent = (FlockAgent.FlockAgent) flockAgent;
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
                

                //var vu = NumberOperations.remap(xMin, xMax, surface.Domain(0).T0,
                //    surface.Domain(0).T1, agent.Velocity.X);
                //var vv = NumberOperations.remap(yMin, yMax, surface.Domain(1).T0,
                //    surface.Domain(1).T1, agent.Velocity.Y);


                surfacePositions.Add(new GH_Point(container.Surface.PointAt(nu, nv)));
                //surfaceVectors.Add(new GH_Vector(new Vector3d(surface.PointAt(vu, vv))));
                //surfacePositions.Add(new GH_Point(surface.PointAt(agent.Position.X, agent.Position.Y)));
            }
            DA.SetDataList("Positions", surfacePositions);
            DA.SetDataList("Velocities", velocities);
            DA.SetDataList("repellers", surfaceRepller);
            DA.SetDataList("testpt", positions);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }
        public override Guid ComponentGuid { get { return new Guid("7066d2ac-3c0a-47db-b22f-4d8c25bf8baa"); } }
    }
}