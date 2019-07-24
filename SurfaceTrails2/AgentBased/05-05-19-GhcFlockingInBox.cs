using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.Utilities;

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
            //pManager.AddBoxParameter("Box", "Box", "Box", GH_ParamAccess.item);
            //pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to modify", GH_ParamAccess.item);
            pManager.AddGenericParameter("Containment", "C", "Containment", GH_ParamAccess.list);

        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "P", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "V", "The agent veloctiies", GH_ParamAccess.list);
            pManager.AddPointParameter("AttractorPoints", "A", "The agent AttractorPoints", GH_ParamAccess.list);
            pManager.AddPointParameter("ClosestPoints", "C", "The agent ClosestPoints", GH_ParamAccess.list);
            //pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to display", GH_ParamAccess.item);
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
            List<FlockAgent.FlockAgent> agents = new List<FlockAgent.FlockAgent>();
            //Box box = Box.Unset;
            var startPoints = new List<Point3d>();

            
            List<GH_Point> surfacePositions = new List<GH_Point>();

            //Mesh mesh = null;
            //var boxContainment = new BoxContainment();
            //var meshContainment = new ContainOutsideMesh();

            List<IAgentContainment> containments = new List<IAgentContainment>();

            var closestPoints = new List<Point3d>();
            //get values from grasshopper
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetDataList("Agents", agents);
            DA.GetDataList("Flock Values", flockProps);
            DA.GetDataList("Interactions", interactions);
            DA.GetDataList("Containment", containments);
            //DA.GetData("Box", ref box);
            //DA.GetData("Mesh", ref mesh);

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
            foreach (FlockAgent.FlockAgent agent in agents)
            {
                foreach (var containment in containments)
                {
                    if (containment.Label == "s")
                    {

                    }
                }
                agent.Containment = containments;
                //agent.IContainment = boxContainment;
                //agent.IContainment2 = meshContainment;
                //boxContainment.Box = box;
                //meshContainment.Mesh = mesh;
                //agent.ClosestPoint = closestPoints[j];
                agent.Interactions = interactions;

                //startPoints.Add(agent.StartPosition);

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
                            //for (int i = 0; i < startPoints.Count; i++)
                            //{
                            //    double t;
                            //    interaction.Curves[0].ClosestPoint(startPoints[i], out t);
                            //    var curveClosestPoint = interaction.Curves[0].PointAt(t);
                            //    closestPoints.Add(curveClosestPoint);

                        

                                //_flockSystem.ClosestPoints.Add(interaction.ClosestPoint);
                            //}
                    }
                    else if (interaction.Label == "w")
                    {
                        _flockSystem.Wind = interaction.WindVec;
                    }
                }

                _flockSystem.UseParallel = iUseParallel;
                 //flockSystem.Box = box;
                 //_flockSystem.CurvePoints = startPoints;
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
                var agent = (FlockAgent.FlockAgent)flockAgent;


                foreach (var containment in containments)
                {
                    if (containment.Label == "s")
                    {
                        var srfContainment =  (SurfaceContainment) containment ;
                        
                        var nu = NumberOperations.remap(srfContainment.xMin, srfContainment.xMax, srfContainment.Surface.Domain(0).T0,
                            srfContainment.Surface.Domain(0).T1, flockAgent.Position.X);
                        var nv = NumberOperations.remap(srfContainment.yMin, srfContainment.yMax, srfContainment.Surface.Domain(1).T0,
                            srfContainment.Surface.Domain(1).T1, flockAgent.Position.Y);

                        surfacePositions.Add(new GH_Point(srfContainment.Surface.PointAt(nu, nv)));
                        positions.AddRange(surfacePositions);
                    }
                    else
                    {

                    }

                }

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
            //DA.SetData("Mesh", mesh);
            DA.SetData("Info", info);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Engine__2_; } }
        public override Guid ComponentGuid { get { return new Guid("d7f22bc3-fc53-4ad9-ae67-6597d259d671"); } }
    }
}