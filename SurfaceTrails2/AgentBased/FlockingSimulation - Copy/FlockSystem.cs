using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.AgentBased
{
    public class FlockSystem
    {
        public List<FlockAgent> Agents;

        public double Timestep;
        public double NeighbourhoodRadius;
        public double AlignmentStrength;
        public double CohesionStrength;
        public double SeparationStrength;
        public double SeparationDistance;
        public List<Circle> Repellers;
        public bool UseParallel;


        public FlockSystem(int agentCount, bool is3D)
        {
            Agents = new List<FlockAgent>();

            if (is3D)
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        PointOperations.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 30.0),
                        VectorOperations.GetRandomUnitVector() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
            else
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        PointOperations.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 0.0),
                        VectorOperations.GetRandomUnitVectorXY() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
        }


        private List<FlockAgent> FindNeighbours(FlockAgent agent)
        {
            List<FlockAgent> neighbours = new List<FlockAgent>();

            foreach (FlockAgent neighbour in Agents)
                if (neighbour != agent && neighbour.Position.DistanceTo(agent.Position) < NeighbourhoodRadius)
                    neighbours.Add(neighbour);

            return neighbours;
        }

        private void ComputeAgentDesiredVelocity(FlockAgent agent)
        {
            List<FlockAgent> neighbours = FindNeighbours(agent);
            agent.ComputeDesiredVelocity(neighbours);
        }


        public void Update()
        {
            if (UseParallel)
                Parallel.ForEach(Agents, ComputeAgentDesiredVelocity);
            else
                foreach (FlockAgent agent in Agents)
                    ComputeAgentDesiredVelocity(agent);

            // Once the desired velocity for each agent has been computed, we update each position and velocity
            foreach (FlockAgent agent in Agents)
                agent.UpdateVelocityAndPosition();
        }


        public void UpdateUsingRTree()
        {
            /* First, build the R-Tree */

            RTree rTree = new RTree();

            for (int i = 0; i < Agents.Count; i++)
                rTree.Insert((Point3d) Agents[i].Position, i);

            /* Then, we use the R-Tree to find the neighbours
                and compute the desired velocity */

            foreach (FlockAgent agent in Agents)
            {
                List<FlockAgent> neighbours = new List<FlockAgent>();

                EventHandler<RTreeEventArgs> rTreeCallback =
                (object sender, RTreeEventArgs args) =>
                {
                    if (Agents[args.Id] != agent)
                        neighbours.Add(Agents[args.Id]);
                };

                rTree.Search(new Sphere(agent.Position, NeighbourhoodRadius), rTreeCallback);

                agent.ComputeDesiredVelocity(neighbours);
            }



            // Once the desired velocity for each agent has been computed, we can update each position and velocity               
            foreach (FlockAgent agent in Agents) agent.UpdateVelocityAndPosition();
        }
    }
}
