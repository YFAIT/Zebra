using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingMapToSurface
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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "Play", "Play", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("3D", "3D", "3D", GH_ParamAccess.item, true);
            pManager.AddSurfaceParameter("srf", "srf", "srf", GH_ParamAccess.item);
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
            pManager.AddBooleanParameter("Use Parallel", "Use Parallel", "Use Parallel", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Use R-Tree", "Use R-Tree", "Use R-Tree", GH_ParamAccess.item, false);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "Positions", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent veloctiies", GH_ParamAccess.list);
            pManager.AddPointParameter("repellers", "repellers", "repellers", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================
            bool iReset = true;
            bool iPlay = false;
            bool i3D = false;
            Surface baseSurface = null;
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

            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetData("3D", ref i3D);
            DA.GetData("srf", ref baseSurface);
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

            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();
            List<GH_Point> surfacePositions = new List<GH_Point>();
            List<Circle> surfaceRepller = new List<Circle>();
            List<Circle> surfaceAttractors = new List<Circle>();
            var surface = baseSurface.ToNurbsSurface();

      
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(iCount,surface);
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
                        surface.Domain(0).T1, flockSystem.XMin, flockSystem.XMax, u);
                    var nv = NumberOperations.remap(surface.Domain(1).T0,
                        surface.Domain(1).T1, flockSystem.YMin, flockSystem.YMax, v);
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
                        surface.Domain(0).T1, flockSystem.XMin, flockSystem.XMax, u);
                    var nv = NumberOperations.remap(surface.Domain(1).T0,
                        surface.Domain(1).T1, flockSystem.YMin, flockSystem.YMax, v);
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
            foreach (FlockAgent agent in flockSystem.Agents)
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

                var nu = NumberOperations.remap(flockSystem.XMin, flockSystem.XMax, surface.Domain(0).T0,
                    surface.Domain(0).T1, agent.Position.X);
                var nv = NumberOperations.remap(flockSystem.YMin, flockSystem.YMax, surface.Domain(1).T0,
                    surface.Domain(1).T1, agent.Position.Y);

                surfacePositions.Add(new GH_Point(surface.PointAt(nu, nv)));
                //surfacePositions.Add(new GH_Point(surface.PointAt(agent.Position.X, agent.Position.Y)));
            }


            DA.SetDataList("Positions", surfacePositions);
            DA.SetDataList("Velocities", velocities);
            DA.SetDataList("repellers", surfaceRepller);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }
        public override Guid ComponentGuid { get { return new Guid("7066d2ac-3c0a-47db-b22f-4d8c25bf8baa"); } }
    }
}