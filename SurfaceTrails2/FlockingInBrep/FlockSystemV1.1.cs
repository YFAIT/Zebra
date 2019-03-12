using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.FlockingInBrep
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
        public Brep Brep;
        Random random = new Random();
        public Mesh Mesh= new Mesh();
        public Vector3d Wind;

        public BoundingBox BBox { get; set; }  
        public Point3d Min { get; set; }
        public Point3d Max { get; set; }
        public FlockSystem(int agentCount,Brep brep)
        {
            Brep = brep;
            Agents = new List<FlockAgent>();

            //var box = Box.Unset;
            //brep.GetBoundingBox(Plane.WorldXY, out box);
            var box = brep.GetBoundingBox(true);

            var min = box.PointAt(0,0,0);
            var max = box.PointAt(1, 1, 1);

            BBox = box;
            Min = min;
            Max = max;

            var mesh = Mesh.CreateFromBrep(brep);
            for (int i = 0; i < mesh.Length; i++)
            {
                Mesh.Append(mesh[i]);
            }
            Mesh.Weld(0.01);

            for (int i = 0; i < agentCount; i++)
                //Parallel.For(0, agentCount, (i, loopState) =>
            {
                //var randPt = Util.GetRandomPoint(min.X, max.Y, min.Z, max.X, min.Y, max.Z);
                var randPt = box.PointAt(random.NextDouble(), random.NextDouble(), random.NextDouble());


                if (Mesh.IsPointInside(randPt, 0.01, false))
                {
                    FlockAgent agent = new FlockAgent(
                        randPt,
                        Util.GetRandomUnitVector() /** 4.0*/);
                    agent.FlockSystem = this;
                    Agents.Add(agent);
                }

                if (Agents.Count == agentCount)
                {
                    //loopState.Break();
                    break;
                }

                //});
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
