using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SurfaceTrails2.AgentBased.FlockingInBox
{
    public class GhcFlockingSimulation : GH_Component
    {
        private FlockSystem _flockSystem;
       

        public GhcFlockingSimulation()
            : base(
                  "Flocking in Box",
                  "Flocking in Box",
                  "Flocking in Box",
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
            pManager.AddPointParameter("AttractorCurvePoints", "AttractorCurvePoints", "AttractorCurvePoints", GH_ParamAccess.list);
            pManager[14].Optional = true;
            pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            pManager[15].Optional = true;
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddBoxParameter("Box", "Box", "Box", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to modify", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "Positions", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent veloctiies", GH_ParamAccess.list);
            pManager.AddPointParameter("AttractorPoints", "AttractorPoints", "The agent AttractorPoints", GH_ParamAccess.list);
            pManager.AddPointParameter("ClosestPoints", "ClosestPoints", "The agent ClosestPoints", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to display", GH_ParamAccess.item);

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
            Vector3d wind = Vector3d.Unset;
            var agents = new List<FlockAgent>();
            Box box = Box.Unset;
            var startPoints = new List<Point3d>();
            Mesh mesh = null;

            var boxContainment = new BoxContainment();
            var meshContainment = new MeshWindForming();



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
            DA.GetDataList("AttractorCurvePoints", startPoints);
            DA.GetData("Wind", ref wind);
            DA.GetDataList("Agents", agents);
            DA.GetData("Box", ref box);
            DA.GetData("Mesh", ref mesh);

            


            var closestPoints = new List<Point3d>();
            //closest Points to startpoints
            for (int i = 0; i < startPoints.Count; i++)
            {
                double t;
                iAttractorCurves[0].ClosestPoint(startPoints[i], out t);
                var curveClosestPoint = iAttractorCurves[0].PointAt(t);
                closestPoints.Add(curveClosestPoint);
            }
            //Random random = new Random();
            //var randPt = box.PointAt(random.NextDouble(), random.NextDouble(), random.NextDouble());
            //BoxAgent fagent = new BoxAgent(randPt, Util.GetRandomUnitVector() * 4.0);
            
            int j = 0;
            foreach (FlockAgent agent in agents)
            {
                agent.IContainment = boxContainment;
                agent.IContainment2 = meshContainment;
                boxContainment.Box = box;
                meshContainment.Mesh = mesh;
                agent.ClosestPoint = closestPoints[j];
                j++;
            }
            var ifagents = new List<IFlockAgent>();
            ifagents.AddRange(agents);
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================
            if (iReset || _flockSystem == null)
            {
                _flockSystem = new FlockSystem(/*iCount,*//* i3D,*/ /*box,*/ /*fagent*/ifagents);
            }
            else
            {
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================
                _flockSystem.Timestep = iTimestep;
                _flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                _flockSystem.AlignmentStrength = iAlignment;
                _flockSystem.CohesionStrength = iCohesion;
                _flockSystem.SeparationStrength = iSeparation;
                _flockSystem.SeparationDistance = iSeparationDistance;
                _flockSystem.Repellers = iRepellers;
                _flockSystem.Attractors = iAttractors;
                _flockSystem.AttractorCurves = iAttractorCurves;
                _flockSystem.UseParallel = iUseParallel;
                //flockSystem.Box = box;
                _flockSystem.Wind = wind;
                _flockSystem.CurvePoints = startPoints;
                _flockSystem.ClosestPoints = closestPoints;

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
                var agent = (FlockAgent) flockAgent;
                positions.Add(new GH_Point(agent.Position));
                velocities.Add(new GH_Vector(agent.Velocity));

            }

            DA.SetDataList("Positions", positions);
            DA.SetDataList("Velocities", velocities);
            DA.SetDataList("AttractorPoints", startPoints);
            DA.SetDataList("ClosestPoints", closestPoints);
            DA.SetData("Mesh", mesh);
        }


        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }


        public override Guid ComponentGuid { get { return new Guid("d7f22bc3-fc53-4ad9-ae67-6597d259d671"); } }
    }
}