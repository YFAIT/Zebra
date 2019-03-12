using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SurfaceTrails2.FlockingInPlane
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
        public List<Curve> InnerCurves;
        public Curve Curve;

        public bool UseParallel;
        public BoundingBox Box ;
        Random random = new Random();


        public FlockSystem(int agentCount/*, bool is3D*//*, Box box*/,Curve curve)
        {
            Agents = new List<FlockAgent>();
            this.Curve = curve;
           var box = curve.GetBoundingBox(true);
            Box = box;
            var min= box.Corner(true, true, true);
            var max = box.Corner(false, false,true);
            var randX = random.NextDouble() * (max.X - min.X) + min.X;
            var randY = random.NextDouble() * (max.Y - min.Y) + min.Y;

            //if (is3D)
            //    for (int i = 0; i < agentCount; i++)
            //    {
            //        var randPt = box.PointAt(random.NextDouble(), random.NextDouble(), random.NextDouble());

            //        FlockAgent agent = new FlockAgent(
            //        //Util.GetRandomPoint(Box.PointAt(0,0,0).X, Box.PointAt(0, 0, 0).Y, Box.PointAt(0, 0, 0).Z, Box.PointAt(1, 1, 1).X, Box.PointAt(1, 1, 1).Y, Box.PointAt(1,1,1).Z),

            //            randPt,
            //        Util.GetRandomUnitVector() * 4.0);

            //        agent.FlockSystem = this;

            //        Agents.Add(agent);
            //    }
            //else
            for (int i = 0; i < agentCount; i++)
            {
                var randPt = Util.GetRandomPoint(min.X, max.X, min.Y, max.Y, 0.0, 0.0);
                //var randPt = new Point3d(randX,randY,0);


                FlockAgent agent = new FlockAgent(randPt, Util.GetRandomUnitVectorXY() * 4.0);
                    agent.FlockSystem = this;

                 if(curve.Contains(randPt) == PointContainment.Inside)
                    Agents.Add(agent);

                 if (Agents.Count == agentCount)
                    break;

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
                (sender, args) =>
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
