﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SurfaceTrails2.AgentBased
{
    public class FlockSystem
    {
        public List<IFlockAgent> IAgents = new List<IFlockAgent>();
        public double Timestep;
        public double NeighbourhoodRadius;
        public double AlignmentStrength;
        public double CohesionStrength;
        public double SeparationStrength;
        public double SeparationDistance;
        public List<Circle> Repellers;
        public List<Circle> Attractors;
        public List<Curve> AttractorCurves;
        public List<Point3d> CurvePoints;
        public List<Point3d> ClosestPoints;
        public bool UseParallel;
        public Vector3d Wind;
        private  List<IFlockAgent> _iAgents = new List<IFlockAgent>();

        public FlockSystem(List<IFlockAgent> Iagents)
        {
            _iAgents = Iagents; 

            for (int i = 0; i <_iAgents.Count; i++)
                {
                    _iAgents[i].FlockSystem = this;
                    IAgents.Add(_iAgents[i]);
                }
        }

        private List<IFlockAgent> FindNeighbours(IFlockAgent agent)
        {
            List<IFlockAgent> neighbours = new List<IFlockAgent>();

            foreach (IFlockAgent neighbour in IAgents)
                if (neighbour != agent && neighbour.Position.DistanceTo(agent.Position) < NeighbourhoodRadius)
                    neighbours.Add(neighbour);

            return neighbours;
        }
        private void ComputeAgentDesiredVelocity(IFlockAgent agent)
        {
            List<IFlockAgent> neighbours = FindNeighbours(agent);
            agent.ComputeDesiredVelocity(neighbours);
        }
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
