﻿using Rhino.Geometry;

namespace SurfaceTrails2.AgentBased
{
    //Interface for agents to be contained in any geometry it varies based on geometry type
    public interface IAgentContainment
    {
        string Label { get; set; }
        double Multiplier { get; set; }
        Vector3d DesiredVector(Point3d position, Vector3d desiredVelocity);
    }
}