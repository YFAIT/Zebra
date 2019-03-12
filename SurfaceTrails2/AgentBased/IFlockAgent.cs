﻿using System.Collections.Generic;
using Rhino.Geometry;
using SurfaceTrails2.AgentBased.FlockingInBox;

namespace SurfaceTrails2.FlockingInBox
{
    public interface IFlockAgent
    {
        Point3d Position
        {
            get;
            set;
        }
        Vector3d Velocity
        {
            get;
            set;
        }
        FlockSystem FlockSystem
        {
            get;
            set;
        }
        void UpdateVelocityAndPosition();
        void ComputeDesiredVelocity(List<IFlockAgent> neighbours);
    }
}