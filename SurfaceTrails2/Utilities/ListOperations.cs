using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace SurfaceTrails2.Utilities
{
    public static class ListOperations
    {
        public static GH_Structure<T> ReOrganize<T>(GH_Structure<T> tree) where T : IGH_Goo
        {
            for (int i = 0; i < tree.PathCount; i++)
            {
                var temp1 = tree.get_Branch(i)[2];
                var temp2 = tree.get_Branch(i)[3];
                tree.get_Branch(i)[2] = temp2;
                tree.get_Branch(i)[3] = temp1;
            }
            return tree;
        }
        public static DataTree<T> ReOrganize2<T>(DataTree<T> tree)
        {
            for (int i = 0; i < tree.BranchCount; i++)
            {
                var temp1 = tree.Branch(i)[2];
                var temp2 = tree.Branch(i)[3];
                tree.Branch(i)[2] = temp2;
                tree.Branch(i)[3] = temp1;

                temp1 = tree.Branch(i)[4];
                temp2 = tree.Branch(i)[5];

                tree.Branch(i)[4] = temp2;
                tree.Branch(i)[5] = temp1;
            }
            return tree;
        }

        public static GH_Structure<T> PartitionToGH_Structure<T>(List<T> list, int count) where T : IGH_Goo
        {
            var tree = new GH_Structure<T>();
            int j = 0;
            int k = 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (k == 2)
                    tree.Append(list[i+1], new GH_Path(j));
                if (k == 3)
                    tree.Append(list[i - 1], new GH_Path(j));

                if (i % count == 0 && i != 0)
                {
                    tree.Append(list[i], new GH_Path(j));
                    j++;
                    k = 0;
                }
                if (j < list.Count / count)
                    tree.Append(list[i], new GH_Path(j));

            }
            return tree;
        }
        public static DataTree<T> PartitionToTree<T>(List<T> list, int partitions)
        {
            var tree = new DataTree<T>();
            int index = 0;
            int count = 0;

            for (int i = 0; i < list.Count; i++)
            {
                tree.Add(list[i], new GH_Path(index));
                count++;
                if (count >= partitions)
                {
                    count = 0;
                    index++;
                }
            }
            return tree;
        }
        public static T[] Shift<T>(IList<T> input, int shift)   
        {
            int l = input.Count;

            if (shift < 0)
                shift = (shift % l) + l;

            T[] result = new T[l];
            for (int i = 0; i < l; i++)
            {
                result[i] = input[(shift + i) % l];
            }
            return result;
        }
    }
}
