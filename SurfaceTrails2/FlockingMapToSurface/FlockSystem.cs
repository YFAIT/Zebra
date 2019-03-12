using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingMapToSurface
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
        public List<Circle> Attractors;
        public bool UseParallel;
        private double _xMax;
        private double _xMin;
        private double _yMax;
        private double _yMin;

        public double XMax { get { return _xMax; } set { _xMax = value; } }
        public double XMin { get { return _xMin; } set { _xMin = value; } }
        public double YMax { get { return _yMax; } set { _yMax = value; } }
        public double YMin { get { return _yMin; } set { _yMin = value; } }
        public FlockSystem(int agentCount, NurbsSurface surface)
        {
            Agents = new List<FlockAgent>();
            //surface.Transpose(true);
            var boundingBox = surface.GetBoundingBox(true);
            //_xMin = surface.Domain(0).T0;
            //_xMax = surface.Domain(0).T1;
            //_yMin = surface.Domain(1).T0;
            //_yMax = surface.Domain(1).T1;
            _xMin = boundingBox.Corner(true, true, true).X;
            _xMax = boundingBox.Corner(false, true, true).X;
            _yMin = boundingBox.Corner(true, true, true).Y;
            _yMax = boundingBox.Corner(true, false, true).Y;

            for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        Util.GetRandomPoint(_xMin, _xMax, _yMin, _yMax, 0.0, 0.0),
                        Util.GetRandomUnitVectorXY() * 4.0);

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
                rTree.Insert(Agents[i].Position, i);

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
