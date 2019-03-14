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
                  "Flocking in Plane",
                  "Flocking in Plane",
                  "Flocking in Plane",
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
            pManager.AddCurveParameter("AttractorCurves", "AttractorCurves", "AttractorCurves", GH_ParamAccess.list);
            pManager[13].Optional = true;
            pManager.AddCurveParameter("innercrv", "innercrv", "innercrv", GH_ParamAccess.list);
            pManager[14].Optional = true;
            pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            pManager[15].Optional = true;
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddCurveParameter("crv", "crv", "crv", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "Positions", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent veloctiies", GH_ParamAccess.list);
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
            List<Curve> iAttractorCurves = new List<Curve>();
            List<Curve> iInnerCurves = new List<Curve>();
            Vector3d wind = Vector3d.Unset;
            var Agents = new List<FlockAgent>();
            Curve curve = null;

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
            DA.GetDataList("AttractorCurves", iAttractorCurves);
            DA.GetDataList("innercrv", iInnerCurves);
            DA.GetData("Wind", ref wind);
            DA.GetDataList("Agents", Agents);
            DA.GetData("crv", ref curve);

            Random random = new Random();
            var agents = new List<FlockAgent>();
            var box = curve.GetBoundingBox(true);

            var min = box.Corner(true, true, true);
            var max = box.Corner(false, false, true);
            var randX = random.NextDouble() * (max.X - min.X) + min.X;
            var randY = random.NextDouble() * (max.Y - min.Y) + min.Y;

            for (int i = 0; i < iCount; i++)
            {
                var randPt = Util.GetRandomPoint(min.X, max.X, min.Y, max.Y, 0.0, 0.0);
                //var randPt = new Point3d(randX,randY,0);


                FlockAgent agent = new FlockAgent(randPt, Util.GetRandomUnitVectorXY() * 4.0);

                if (curve.Contains(randPt) == PointContainment.Inside)
                    agents.Add(agent);

                if (agents.Count == iCount)
                    break;
            }
            foreach (FlockAgent agent in Agents)
            {
                var planeContainment = new PlaneContainment();
                agent.IContainment = planeContainment;
                planeContainment.Curve = curve;

                //agent.InnerCurves = iInnerCurves;
            }
            var Ifagents = new List<IFlockAgent>();
            Ifagents.AddRange(Agents);

            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(Ifagents);
            }
            else
            {
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================

                flockSystem.Timestep = iTimestep;
                flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                flockSystem.AlignmentStrength = iAlignment;
                //flockSystem.InnerCurves = iInnerCurves;
                flockSystem.CohesionStrength = iCohesion;
                flockSystem.SeparationStrength = iSeparation;
                flockSystem.SeparationDistance = iSeparationDistance;
                flockSystem.Repellers = iRepellers;
                flockSystem.Attractors = iAttractors;
                flockSystem.UseParallel = iUseParallel;
                flockSystem.AttractorCurves = iAttractorCurves;
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

            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();

            foreach (FlockAgent agent in flockSystem.IAgents)
            {
                positions.Add(new GH_Point(agent.Position));
                velocities.Add(new GH_Vector(agent.Velocity));
            }

            DA.SetDataList("Positions", positions);
            DA.SetDataList("Velocities", velocities);
        }


        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }


        public override Guid ComponentGuid { get { return new Guid("48b00dff-fb37-474d-8ec6-5b888e9f28d8"); } }
    }
}