using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SurfaceTrails2.Properties;

namespace SurfaceTrails2.Utilities
{
    public class Partition : GH_Component
    {
        public Partition()
            : base("Partition test", "PartTest", "Test partitioning logic.", "Zebra", "Utilities")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Generic data", GH_ParamAccess.tree);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Partitioned data", "P", "Partitioned data into chunks of 8.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess access)
        {
            var tree = new GH_Structure<IGH_Goo>();

            access.GetDataTree(0, out  tree);

            var partitioned = new GH_Structure<IGH_Goo>();
            if (tree != null && !tree.IsEmpty)
            {
                for (int p = 0; p < tree.PathCount; p++)
                {
                    var path = tree.Paths[p];
                    var list = tree.Branches[p];
                    int index = 0;
                    int count = 0;

                    for (int i = 0; i < list.Count; i++)
                    {
                        partitioned.Append(list[i], path.AppendElement(index));

                        count++;
                        if (count >= 8)
                        {
                            count = 0;
                            index++;
                        }
                    }
                }
            }
            access.SetDataTree(0, partitioned);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.PartitionList; }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{5DBD9E06-4ACB-44E3-BEB6-46D52124EB95}"); }
        }
    }
}