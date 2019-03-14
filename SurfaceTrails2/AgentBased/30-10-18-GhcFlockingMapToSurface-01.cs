using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased
{
    public class GhcFlockingSimulation : GH_Component
    {
        private FlockSystem flockSystem;

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
            pManager.AddIntegerParameter("Count", "Count", "Number of Agents", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Timestep", "Timestep", "Timestep", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Neighbourhood Radius", "Neighbourhood Radius", "Neighbourhood Radius", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("Alignment", "Alignment", "Alignment", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Cohesion", "Cohesion", "Cohesion", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation", "Separation", "Separation", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation Distance", "Separation Distance", "Separation Distance", GH_ParamAccess.item, 1.5);
            pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            pManager[11].Optional = true;
            pManager.AddCircleParameter("Attractors", "Attractors", "Attractors", GH_ParamAccess.list);
            pManager[12].Optional = true;
            //pManager.AddCurveParameter("AttractorCurves", "AttractorCurves", "AttractorCurves", GH_ParamAccess.list);
            //pManager[13].Optional = true;
            //pManager.AddPointParameter("AttractorCurvePoints", "AttractorCurvePoints", "AttractorCurvePoints", GH_ParamAccess.list);
            //pManager[14].Optional = true;
            pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            //pManager[15].Optional = true;
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("srf", "srf", "srf", GH_ParamAccess.item);

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
            int iCount = 0;
            double iTimestep = 0.0;
            double iNeighbourhoodRadius = 0.0;
            double iAlignment = 0.0;
            double iCohesion = 0.0;
            double iSeparation = 0.0;
            double iSeparationDistance = 0.0;
            List<Circle> iRepellers = new List<Circle>();
            List<Circle> iAttractors = new List<Circle>();
            //List<Curve> iAttractorCurves = new List<Curve>();
            Vector3d wind = Vector3d.Unset;
            List<FlockAgent> agents = new List<FlockAgent>();
            Surface baseSurface = null;
            //var startPoints = new List<Point3d>();


            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetData("Count", ref iCount);
            DA.GetData("Timestep", ref iTimestep);
            DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius);
            DA.GetData("Alignment", ref iAlignment);
            DA.GetData("Cohesion", ref iCohesion);
            DA.GetData("Separation", ref iSeparation);
            DA.GetData("Separation Distance", ref iSeparationDistance);
            DA.GetDataList("Repellers", iRepellers);
            DA.GetDataList("Attractors", iAttractors);
            //DA.GetDataList("AttractorCurves", iAttractorCurves);
            //DA.GetDataList("AttractorCurvePoints", startPoints);
            DA.GetData("Wind", ref wind);
            DA.GetDataList("Agents", agents);
            DA.GetData("srf", ref baseSurface);

            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();
            List<GH_Point> surfacePositions = new List<GH_Point>();
            List<Circle> surfaceRepller = new List<Circle>();
            List<Circle> surfaceAttractors = new List<Circle>();
            var surface = baseSurface.ToNurbsSurface();


            //var boundingBox = surface.GetBoundingBox(true);
            //var xMin = boundingBox.Corner(true, true, true).X;
            //var xMax = boundingBox.Corner(false, true, true).X;
            //var yMin = boundingBox.Corner(true, true, true).Y;
            //var yMax = boundingBox.Corner(true, false, true).Y;

            var xMin = 0;
            var xMax = 30;
            var yMin = 0;
            var yMax = 30;

            //for (int i = 0; i < iCount; i++)
            //{
            //    BoxAgent agent = new BoxAgent(
            //        Util.GetRandomPoint(xMin, xMax, yMin, yMax, 0.0, 0.0),
            //        Util.GetRandomUnitVectorXY() * 4.0);

            //    agents.Add(agent);
            //}



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

            int j = 0;
            foreach (FlockAgent agent in agents)
            {
                //double u;
                //double v;
                //surface.ClosestPoint(agent.Position, out u, out v);
                //var nu = NumberOperations.remap(surface.Domain(0).T0,
                //    surface.Domain(0).T1, xMin, xMax, u);
                //var nv = NumberOperations.remap(surface.Domain(1).T0,
                //    surface.Domain(1).T1, yMin, yMax, v);

                //agent.Position = new Point3d(nu, nv, 0);
                agent.Position = new Point3d(agent.Position.X, agent.Position.Y, 0);

                var surfaceContainment = new SurfaceContainment();
                agent.IContainment = surfaceContainment;

                surfaceContainment.Surface = surface;

                surfaceContainment.xMin = xMin;
                surfaceContainment.xMax = xMax;
                surfaceContainment.yMin = yMin;
                surfaceContainment.yMax = yMax;

                //agent.ClosestPoint = closestPoints[j];
                j++;
            }
            var Ifagents = new List<IFlockAgent>();
            Ifagents.AddRange(agents);
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(/*iCount,surface*/Ifagents);
            }
            else
            {
                // ===============================================================================================
                // Remap repellers
                // ===============================================================================================
                foreach (Circle repeller in iRepellers)
                {
                    double u;
                    double v;
                    surface.ClosestPoint(repeller.Center, out u, out v);

                    var nu = NumberOperations.remap(surface.Domain(0).T0,
                        surface.Domain(0).T1, xMin, xMax, u);
                    var nv = NumberOperations.remap(surface.Domain(1).T0,
                        surface.Domain(1).T1, yMin, yMax, v);
                    Point3d remappedCenter = new Point3d(nu, nv, 0);
                    Circle remappedCircle = new Circle(remappedCenter, repeller.Radius);
                    surfaceRepller.Add(remappedCircle);

                }
                // ===============================================================================================
                // Remap Attractors
                // ===============================================================================================
                foreach (Circle attractor in iAttractors)
                {
                    double u;
                    double v;
                    surface.ClosestPoint(attractor.Center, out u, out v);

                    var nu = NumberOperations.remap(surface.Domain(0).T0,
                        surface.Domain(0).T1, xMin, xMax, u);
                    var nv = NumberOperations.remap(surface.Domain(1).T0,
                        surface.Domain(1).T1, yMin, yMax, v);
                    Point3d remappedCenter = new Point3d(nu, nv, 0);
                    Circle remappedCircle = new Circle(remappedCenter, attractor.Radius);
                    surfaceAttractors.Add(remappedCircle);
                }
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================
                flockSystem.Timestep = iTimestep;
                flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                flockSystem.AlignmentStrength = iAlignment;
                flockSystem.CohesionStrength = iCohesion;
                flockSystem.SeparationStrength = iSeparation;
                flockSystem.SeparationDistance = iSeparationDistance;
                flockSystem.Repellers = surfaceRepller;
                flockSystem.Attractors = surfaceAttractors;
                flockSystem.UseParallel = iUseParallel;
                //flockSystem.AttractorCurves = iAttractorCurves;
                flockSystem.Wind = wind;
                // ===============================================================================
                // Update the flock
                // ===============================================================================
                if (iUseRTree)
                    flockSystem.UpdateUsingRTree();
                else
                    flockSystem.Update();

                if (iPlay) ExpireSolution(true);
            }
            // ===============================================================================
            // Output the agent positions and velocities so we can see them on display
            // ===============================================================================
            foreach (FlockAgent agent in flockSystem.IAgents)
            {
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

                var nu = NumberOperations.remap(xMin, xMax, surface.Domain(0).T0,
                    surface.Domain(0).T1, agent.Position.X);
                var nv = NumberOperations.remap(yMin, yMax, surface.Domain(1).T0,
                    surface.Domain(1).T1, agent.Position.Y);

                surfacePositions.Add(new GH_Point(surface.PointAt(nu, nv)));
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