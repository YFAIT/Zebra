using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.Behaviours;
using SurfaceTrails2.AgentBased.Containment;
using SurfaceTrails2.AgentBased.FlockAgent;

/*This Class is the main grasshopper component fo flocking that gets supplied by options to change the flocking attributes
 Also this class controls the timing for the whole flocking algorithm
 */
namespace SurfaceTrails2.AgentBased
{
    public class GhcFlockingInBox : GH_Component
    {
        private FlockSystem _flockSystem;

        public GhcFlockingInBox()
            : base(
                  "Flocking Engine",
                  "Engine",
                  "The main engine to control the flock",
                  "Zebra",
                  "AgentBased")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Use Parallel", "P",
                "If you want to use more than one processor core mark it as true", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Use R-Tree", "T",
                "a faster method to calculate flocking, mark it as true for faster calculations", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "R", "Resets the flock to its initial position", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "P", "set true to start flocking and false to stop it", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Agents", "A", "Agents from the agents component", GH_ParamAccess.list);
            pManager.AddNumberParameter("Flock Properties", "F", "Flock Properties from the properties component", GH_ParamAccess.list);
            pManager.AddGenericParameter("Behaviours", "B", "Behviours should all be supplied to this input", GH_ParamAccess.list);
            pManager[6].Optional = true;
            pManager.AddGenericParameter("Containment", "C",
                "All containing geometry should be supplied in this input, if there is a surface please add it first", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "P", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "V", "The agent veloctiies", GH_ParamAccess.list);
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
            List<IAgentBehaviours> interactions = new List<IAgentBehaviours>();
            List<FlockAgent.FlockAgent> agents = new List<FlockAgent.FlockAgent>();
            List<IAgentContainment> containments = new List<IAgentContainment>();
            //get values from grasshopper
            DA.GetData("Use Parallel", ref iUseParallel);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetDataList("Agents", agents);
            DA.GetDataList("Flock Properties", flockProps);
            DA.GetDataList("Behaviours", interactions);
            DA.GetDataList("Containment", containments);
            //surface params
            var points = agents.Select(p => p.Position).ToList();
            BoundingBox box = new BoundingBox(points);
            //assigning values to flock agents
            foreach (FlockAgent.FlockAgent agent in agents)
            {
                agent.Containment = containments;
                agent.Interactions = interactions;

                if (containments[0].Label == 's')
                    agent.SrfBoudningBox = box;
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
                    agent.Position = agent.StartPosition;
            }
            else
            {
// ===============================================================================================
// Assign the input parameters to the corresponding variables in the  "flockSystem" object
// ===============================================================================================
                 _flockSystem.Timestep = flockProps[0];
                 _flockSystem.NeighbourhoodRadius = flockProps[1];
                 _flockSystem.AlignmentStrength = flockProps[2];
                 _flockSystem.CohesionStrength = flockProps[3];
                 _flockSystem.SeparationStrength = flockProps[4];
                 _flockSystem.SeparationDistance = flockProps[5];
                 _flockSystem.UseParallel = iUseParallel;
// ===============================================================================
// Update the flock
// ===============================================================================
                if (iUseRTree)
                _flockSystem.UpdateUsingRTree();
            else
                _flockSystem.Update();
            //makes grasshopper iterate again when calculation is done
            if (iPlay) ExpireSolution(true);
            }
// ===============================================================================
// Output the agent positions and velocities so we can see them on display
// ===============================================================================
            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();

            foreach (FlockAgent.FlockAgent agent in agents)
            {
                agent.DisplayToGrasshopper();
                positions.Add(agent.GHPosition);
                velocities.Add(agent.GHVelocity);
            }
// ===============================================================================================
// Exporting Data to Grasshopper
// ===============================================================================================
            DA.SetDataList("Positions", positions);
                DA.SetDataList("Velocities", velocities);
        }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Engine__2_; } }
        public override Guid ComponentGuid { get { return new Guid("d7f22bc3-fc53-4ad9-ae67-6597d259d671"); } }
    }
}