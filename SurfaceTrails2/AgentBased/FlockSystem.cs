using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.FlockAgent;
/*This class manages the interaction between each agent in the flock and each other
this class may contain items for behaviours like repellers and attractors and curves, they should be put in a seperate class
 for a better practice */
namespace SurfaceTrails2.AgentBased
{
    public class FlockSystem
    {
// ===============================================================================================
// Read input parameters
// ===============================================================================================
        public bool UseParallel;
        public double Timestep;
        public double NeighbourhoodRadius;
        public double AlignmentStrength;
        public double CohesionStrength;
        public double SeparationStrength;
        public double SeparationDistance;
        public List<Circle> Repellers;
        public List<Circle> Attractors;
        public List<Circle> FollowAttractors;
        public List<Circle> FollowCurveAttractors;
        public List<Curve> AttractorCurves;
        public Vector3d Wind;
        public List<IFlockAgent> IAgents = new List<IFlockAgent>();
        private List<IFlockAgent> _iAgents = new List<IFlockAgent>();
        //Constructor to assign each flock agent to the flocksystem
        public FlockSystem(List<IFlockAgent> Iagents)
        {
            _iAgents = Iagents; 

            for (int i = 0; i <_iAgents.Count; i++)
                {
                    _iAgents[i].FlockSystem = this;
                    IAgents.Add(_iAgents[i]);
                }
        }
// ===============================================================================================
// method to find nearby neignbours to each agent
// ===============================================================================================
        private List<IFlockAgent> FindNeighbours(IFlockAgent agent)
        {
            List<IFlockAgent> neighbours = new List<IFlockAgent>();

            foreach (IFlockAgent neighbour in IAgents)
                if (neighbour != agent && neighbour.Position.DistanceTo(agent.Position) < NeighbourhoodRadius)
                    neighbours.Add(neighbour);

            return neighbours;
        }
// ===============================================================================================
// method to compute desired velocity of agent based on behaviours applied
// ===============================================================================================
        private void ComputeAgentDesiredVelocity(IFlockAgent agent)
        {
            List<IFlockAgent> neighbours = FindNeighbours(agent);
            agent.ComputeDesiredVelocity(neighbours);
        }
// ===============================================================================================
// update per second for position and velocity
// ===============================================================================================
        public void Update()
        {
            if (UseParallel)
                Parallel.ForEach(IAgents, ComputeAgentDesiredVelocity);
            else
                foreach (IFlockAgent agent in IAgents)
                    ComputeAgentDesiredVelocity(agent);

         // Once the desired velocity for each agent has been computed, we update each position and velocity
            foreach (IFlockAgent agent in IAgents)
                agent.UpdateVelocityAndPosition();
        }
        //initialization of the R tree
        public void UpdateUsingRTree()
        {
            /* First, build the R-Tree */
            RTree rTree = new RTree();

            for (int i = 0; i < IAgents.Count; i++)
                rTree.Insert(IAgents[i].Position, i);
            /* Then, we use the R-Tree to find the neighbours
                and compute the desired velocity */

            foreach (IFlockAgent agent in IAgents)
            {
                List<IFlockAgent> neighbours = new List<IFlockAgent>();

                EventHandler<RTreeEventArgs> rTreeCallback =
                (object sender, RTreeEventArgs args) =>
                {
                    if (IAgents[args.Id] != agent)
                        neighbours.Add(IAgents[args.Id]);
                };

                rTree.Search(new Sphere(agent.Position, NeighbourhoodRadius), rTreeCallback);

                agent.ComputeDesiredVelocity(neighbours);
            }
            // Once the desired velocity for each agent has been computed, we can update each position and velocity               
            foreach (IFlockAgent agent in IAgents) agent.UpdateVelocityAndPosition();
        }
    }
}
