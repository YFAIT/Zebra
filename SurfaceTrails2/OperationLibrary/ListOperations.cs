using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
//This Class contains core methods for any List operation used in my code
namespace SurfaceTrails2.OperationLibrary
{
    public static class ListOperations
    {
// ===============================================================================================
// reorganized certain indices in a list of 4 items
// ===============================================================================================
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
// ===============================================================================================
// reorganized certain indices in a list of 4 itemss
// ===============================================================================================
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
// ===============================================================================================
// makes a partitcioned tree out of a list (not working and kept for reference)
// ===============================================================================================
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
// ===============================================================================================
// makes a partitcioned tree out of a list (works as planned)
// ===============================================================================================
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
// ===============================================================================================
// shifts elements from a list from one index to the next index in a circular behaviour
// ===============================================================================================
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
