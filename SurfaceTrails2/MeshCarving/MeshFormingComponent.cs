using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SurfaceTrails2.Properties;
using SurfaceTrails2.Utilities;

namespace SurfaceTrails2.MeshCarving
{
    public class MeshFormingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshFormingComponent class.
        /// </summary>
        private int _count;
        private int _iInterval;
        private double _iThreshold;
        private List<Point3d> _pts;
        private Mesh _tempMesh = new Mesh();
        private Mesh _tempMesh2 = new Mesh();

        public MeshFormingComponent()
          : base("MeshFormingComponent", "Nickname",
              "Description",
              "YFAtools", "Manipulation")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Input Mesh", "M", "Mesh after carving", GH_ParamAccess.item);
            pManager.AddPointParameter("Attractor Points", "P", "Points to Manipulate the Mesh", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Interval", "I", "Time between iterations", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "T", "The distance from which points begin manipulating the mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "The distance mesh points are moved by if manipulated", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "on off button", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Delete old data", GH_ParamAccess.item);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Carved Mesh", "M", "Mesh after carving", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var mesh = new Mesh();
            var attractorPoints = new List<Point3d>();
            var interval = 0;
            double threshold= 0;
            double distance= 0;
            var run = true;
            var reset = true;

            DA.GetData("Input Mesh", ref mesh);
            DA.GetDataList("Attractor Points", attractorPoints);
            DA.GetData("Interval", ref interval);
            DA.GetData("Threshold", ref threshold);
            DA.GetData("Distance", ref distance);
            DA.GetData("Run", ref run);
            DA.GetData("Reset", ref reset);

            _pts = attractorPoints;
            _iThreshold = threshold;
            _iInterval = interval;

            if (run)
                _count++;
            else
                _count = 0;
            //Recursion Loop
            mesh.Weld(0.01);
            if (_tempMesh2 == null)
                _tempMesh = mesh;
            else
                _tempMesh = _tempMesh2;

            var points = _tempMesh.Vertices;
            var faces = _tempMesh.Faces;
            var pointList = points.ToPoint3dArray().ToList();

            for (int i = 0; i < _count; i++)
            {
                foreach (Point3d pt in _pts)
                {
                    int index;
                    PointOperations.ClosestPointWithIndex(pt, pointList, out index);

                    if (pt.DistanceTo(pointList[index]) <= _iThreshold)
                    {
                        //Move Point
                        Vector3d moveVector = (pointList[index] - pt) / pt.DistanceTo(pointList[index]);
                        moveVector = moveVector * distance;
                        Transform move = Transform.Translation(moveVector);
                        var movedPoint = pointList[index];
                        movedPoint.Transform(move);

                        pointList[index] = movedPoint;
                    }
                }
                //Build constructedMesh
                Mesh constructedMesh = new Mesh();
                constructedMesh.Vertices.AddVertices(pointList);
                constructedMesh.Faces.AddFaces(faces);
                constructedMesh.FaceNormals.ComputeFaceNormals();
                constructedMesh.Normals.ComputeNormals();
                constructedMesh.Compact();

                _tempMesh2 = constructedMesh;
            }
            if (reset)
            {
                _tempMesh2 = mesh;
                _count = 0;
            }
            //Export output to grasshopper
            var a = _tempMesh2;
            DA.SetData("Carved Mesh", a);
    }
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.MeshWindForming_01;
            }
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("74eba1d1-ef53-4599-bcfb-56589ed8f4e4"); }
        }
    }
}