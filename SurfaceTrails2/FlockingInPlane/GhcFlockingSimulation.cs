using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingInPlane
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


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "Play", "Play", GH_ParamAccess.item, false);
            //pManager.AddBooleanParameter("3D", "3D", "3D", GH_ParamAccess.item, true);
            pManager.AddCurveParameter("crv", "crv", "crv", GH_ParamAccess.item);
            pManager.AddCurveParameter("innercrv", "innercrv", "innercrv", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Count", "Count", "Number of Agents", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Timestep", "Timestep", "Timestep", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Neighbourhood Radius", "Neighbourhood Radius", "Neighbourhood Radius", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("Alignment", "Alignment", "Alignment", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Cohesion", "Cohesion", "Cohesion", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation", "Separation", "Separation", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation Distance", "Separation Distance", "Separation Distance", GH_ParamAccess.item, 1.5);
            pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            pManager.AddCircleParameter("Attractors", "Attractors", "Attractors", GH_ParamAccess.list);
            pManager[10].Optional = true;
            pManager.AddBooleanParameter("Use Parallel", "Use Parallel", "Use Parallel", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Use R-Tree", "Use R-Tree", "Use R-Tree", GH_ParamAccess.item, false);
            //pManager.AddBoxParameter("Box", "Box", "Box", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
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

            bool iReset = true;
            bool iPlay = false;
            //bool i3D = false;
            Curve curve = null;
            List<Curve> iInnerCurves = new List<Curve>();
            int iCount = 0;
            double iTimestep = 0.0;
            double iNeighbourhoodRadius = 0.0;
            double iAlignment = 0.0;
            double iCohesion = 0.0;
            double iSeparation = 0.0;
            double iSeparationDistance = 0.0;
            List<Circle> iRepellers = new List<Circle>();
            List<Circle> iAttractors = new List<Circle>();

            bool iUseParallel = false;
            bool iUseRTree = false;
            //Box box = Box.Unset;

            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            //DA.GetData("3D", ref i3D);
            DA.GetData("crv", ref curve);
            DA.GetDataList("innercrv", iInnerCurves);
            DA.GetData("Count", ref iCount);
            DA.GetData("Timestep", ref iTimestep);
            DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius);
            DA.GetData("Alignment", ref iAlignment);
            DA.GetData("Cohesion", ref iCohesion);
            DA.GetData("Separation", ref iSeparation);
            DA.GetData("Separation Distance", ref iSeparationDistance);
            DA.GetDataList("Repellers", iRepellers);
            DA.GetDataList("Attractors", iAttractors);
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            //DA.GetData("Box", ref box);



            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(iCount/*, i3D*//*, box*/, curve);
            }
            else
            {
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================

                flockSystem.Timestep = iTimestep;
                flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                flockSystem.AlignmentStrength = iAlignment;
                flockSystem.InnerCurves = iInnerCurves;
                flockSystem.CohesionStrength = iCohesion;
                flockSystem.SeparationStrength = iSeparation;
                flockSystem.SeparationDistance = iSeparationDistance;
                flockSystem.Repellers = iRepellers;
                flockSystem.Attractors = iAttractors;
                flockSystem.UseParallel = iUseParallel;
                //flockSystem.Box = box;


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

            foreach (FlockAgent agent in flockSystem.Agents)
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