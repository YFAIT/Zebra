using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

//using SurfaceTrails2.FlockingInBrep;

namespace SurfaceTrails2.AgentBased.FlockingInBrep
{
    public class GhcFlockingSimulation : GH_Component
    {
        private FlockSystem flockSystem;

        public GhcFlockingSimulation()
            : base(
                  "Flocking in Brep-2",
                  "Flocking in  Brep-2",
                  "Flocking in  Brep-2",
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
            pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            pManager[13].Optional = true;
            pManager.AddCurveParameter("AttractorCurves", "AttractorCurves", "AttractorCurves", GH_ParamAccess.list);
            pManager[14].Optional = true;
            pManager.AddPointParameter("AttractorCurvePoints", "AttractorCurvePoints", "AttractorCurvePoints", GH_ParamAccess.list);
            pManager[15].Optional = true;
            pManager.AddBrepParameter("Brep", "Brep", "Brep", GH_ParamAccess.item);
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
           
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
            Brep brep = null;
            Vector3d wind = Vector3d.Unset;
            var agents = new List<FlockAgent>();
            var startPoints = new List<Point3d>();
            Mesh Mesh = new Mesh();
            //get values from grasshopper
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
            DA.GetDataList("AttractorCurvePoints", startPoints);
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Brep", ref brep);
            DA.GetData("Wind", ref wind);
            DA.GetDataList("Agents", agents);

            //Random random = new Random();
            //int agentCount = iCount;
            //var fagents = new List<BoxAgent>();

            //var box = brep.GetBoundingBox(true);
            //var min = box.PointAt(0, 0, 0);
            //var max = box.PointAt(1, 1, 1);
            //Mesh from brep
            var mesh = Mesh.CreateFromBrep(brep);
            for (int i = 0; i < mesh.Length; i++)
                Mesh.Append(mesh[i]);
            Mesh.Weld(0.01);

            //closest Points to startpoints
            var closestPoints = new List<Point3d>();
            for (int i = 0; i < startPoints.Count; i++)
            {
                double t;
                iAttractorCurves[0].ClosestPoint(startPoints[i], out t);
                var curveClosestPoint = iAttractorCurves[0].PointAt(t);
                closestPoints.Add(curveClosestPoint);
            }
            //---------------------------------------------------------------


            //var randPt = box.PointAt(random.NextDouble(), random.NextDouble(), random.NextDouble());
            //BoxAgent fagent = new BoxAgent(randPt, Util.GetRandomUnitVector() * 4.0);

            //assign values to agents
            int j = 0;
            foreach (FlockAgent agent in agents)
            {
                var brepContainment = new BrepContainment();
                brepContainment.Mesh = Mesh;
                agent.IContainment = brepContainment;

                agent.ClosestPoint = closestPoints[j];
                j++;
            }
            var Ifagents = new List<IFlockAgent>();
            Ifagents.AddRange(agents);
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================
            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(/*iCount,*//* i3D,*/ /*box,*/ /*fagent*/Ifagents);
            }
            else
            {
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================
                flockSystem.Timestep = iTimestep;
                flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                flockSystem.AlignmentStrength = iAlignment;
                flockSystem.CohesionStrength = iCohesion;
                flockSystem.SeparationStrength = iSeparation;
                flockSystem.SeparationDistance = iSeparationDistance;
                flockSystem.Repellers = iRepellers;
                flockSystem.Attractors = iAttractors;
                flockSystem.AttractorCurves = iAttractorCurves;
                flockSystem.UseParallel = iUseParallel;
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("84e7c5f1-b921-48f1-8a82-ec4bcaa1f825"); }
        }
    }
}