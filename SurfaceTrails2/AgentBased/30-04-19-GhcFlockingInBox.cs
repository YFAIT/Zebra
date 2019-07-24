using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SurfaceTrails2.AgentBased.FlockingInBox
{
    public class GhcFlockingInBox : GH_Component
    {
        private FlockSystem _flockSystem;


        public GhcFlockingInBox()
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
            pManager.AddGenericParameter("Agents", "Agents", "Agents to Flock", GH_ParamAccess.list);
            pManager.AddNumberParameter("Flock Values", "Flock Values", "Flock Values", GH_ParamAccess.list);

            //pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            //pManager[6].Optional = true;
            //pManager.AddCircleParameter("Attractors", "Attractors", "Attractors", GH_ParamAccess.list);
            //pManager[7].Optional = true;
            //pManager.AddCurveParameter("AttractorCurves", "AttractorCurves", "AttractorCurves", GH_ParamAccess.list);
            //pManager[8].Optional = true;
            //pManager.AddPointParameter("AttractorCurvePoints", "AttractorCurvePoints", "AttractorCurvePoints", GH_ParamAccess.list);
            //pManager[9].Optional = true;
            //pManager.AddVectorParameter("Wind", "Wind", "Wind", GH_ParamAccess.item);
            //pManager[10].Optional = true;

            pManager.AddGenericParameter("Interactions", "Interactions", "Interactions", GH_ParamAccess.list);
            pManager[6].Optional = true;


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
            List<double> flockProps = new List<double>();
            List<IAgentBehavioursInteractions> interactions = new List<IAgentBehavioursInteractions>();
            //List<Circle> iRepellers = new List<Circle>();
            //List<Circle> iAttractors = new List<Circle>();
            //List<Curve> iAttractorCurves = new List<Curve>();
            //Vector3d wind = Vector3d.Unset;
            List<FlockAgent> agents = new List<FlockAgent>();
            Box box = Box.Unset;
            var startPoints = new List<Point3d>();
            Mesh mesh = null;
            var boxContainment = new BoxContainment();
            var meshContainment = new MeshWindForming();
            var closestPoints = new List<Point3d>();
            //get values from grasshopper
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetDataList("Agents", agents);
            DA.GetDataList("Flock Values", flockProps);
            //DA.GetDataList("Repellers", iRepellers);
            //DA.GetDataList("Attractors", iAttractors);
            //DA.GetDataList("AttractorCurves", iAttractorCurves);
            //DA.GetDataList("AttractorCurvePoints", startPoints);
            //DA.GetData("Wind", ref wind);

            DA.GetDataList("Interactions", interactions);

            DA.GetData("Box", ref box);
            DA.GetData("Mesh", ref mesh);
            //getting closest points to to each agent
            //for (int i = 0; i < startPoints.Count; i++)
            //{
            //    double t;
            //    iAttractorCurves[0].ClosestPoint(startPoints[i], out t);
            //    var curveClosestPoint = iAttractorCurves[0].PointAt(t);
            //    closestPoints.Add(curveClosestPoint);
            //}


            //Random random = new Random();
            //var randPt = box.PointAt(random.NextDouble(), random.NextDouble(), random.NextDouble());
            //BoxAgent fagent = new BoxAgent(randPt, Util.GetRandomUnitVector() * 4.0);
            
            //assigning values to flock agents
            int j = 0;
            foreach (FlockAgent agent in agents)
            {
                agent.IContainment = boxContainment;
                agent.IContainment2 = meshContainment;
                boxContainment.Box = box;
                meshContainment.Mesh = mesh;
                //agent.ClosestPoint = closestPoints[j];
                agent.Interactions = interactions;
                j++;
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




               /* _flockSystem.Repellers = interactions[0].Circles;*///test

                // _flockSystem.Attractors = iAttractors;
                // _flockSystem.AttractorCurves = iAttractorCurves;
                //_flockSystem.Wind = wind;

                foreach (var interaction in interactions)
                {
                    if (interaction.Label == "r")
                        _flockSystem.Repellers= interaction.Circles;
                    else if (interaction.Label == "a")
                        _flockSystem.Attractors= interaction.Circles;
                    else if (interaction.Label == "c")
                    {
                            _flockSystem.AttractorCurves = interaction.Curves;
                            //getting closest points to to each agent
                            for (int i = 0; i < startPoints.Count; i++)
                            {
                                double t;
                                interaction.Curves[0].ClosestPoint(startPoints[i], out t);
                                var curveClosestPoint = interaction.Curves[0].PointAt(t);
                                closestPoints.Add(curveClosestPoint);

                                _flockSystem.ClosestPoints = closestPoints;
                            }
                    }
                    else if (interaction.Label == "w")
                    {
                        _flockSystem.Wind = interaction.WindVec;
                    }
                }

                _flockSystem.UseParallel = iUseParallel;
                 //flockSystem.Box = box;
                 _flockSystem.CurvePoints = startPoints;
                 //_flockSystem.ClosestPoints = closestPoints;
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
            //information check
            var info = "values are" + flockProps[0] + " " + flockProps[1] + " " + flockProps[2] + " " + flockProps[3] + " " +
                          flockProps[4] + " " + flockProps[5] + " " + flockProps[6] + " " + agents[0].MinVelocity + " " + agents[0].MaxVelocity;
            //Export data to grasshopper
            DA.SetDataList("Positions", positions);
            DA.SetDataList("Velocities", velocities);
            DA.SetDataList("AttractorPoints", startPoints);
            DA.SetDataList("ClosestPoints", closestPoints);
            DA.SetData("Mesh", mesh);
            DA.SetData("Info", info);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources._28_8_18_FlockSimulation; } }
        public override Guid ComponentGuid { get { return new Guid("d7f22bc3-fc53-4ad9-ae67-6597d259d671"); } }
    }
}