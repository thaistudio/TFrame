using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame.TTools
{
    public class ListTools
    {

        /// <summary>
        /// Use this method to compare 2 lists. The differences will be saved in a 3rd list.
        /// </summary>
        /// <param name="L1">The first list</param> 
        /// <param name="L2">The second list</param>yu
        /// <param name="L3">Differences between L1 and L2 will be added to L3</param>
        /// <param name="prop"></param>
        public static void CompareList<T>(List<T> L1, List<T> L2, List<T> L3, string prop)
        {
            ListTools listTools = new ListTools();
            if (L2.Count > 0 && L1.Count == L2.Count)
            {
                bool IsDuplicated = false;
                foreach (T t2 in L2)
                {
                    foreach (T t1 in L1)
                    {
                        var v1 = t1.GetType().GetProperty(prop).GetValue(t1);
                        var v2 = t2.GetType().GetProperty(prop).GetValue(t2);
                        if (((IComparable)v1).Equals((IComparable)v2))
                        {
                            IsDuplicated = true;
                            break;
                        }
                    }
                    if (!IsDuplicated) L3.Add(t2);
                }
            }

            else if (L2.Count > 0 && L1.Count > L2.Count)
            {
                foreach (T t in L1.Except(L2).ToList())
                    L3.Add(t);
            }
            else if (L2.Count > 0 && L2.Count > L1.Count)
            {
                foreach (T t in L1.Except(L2).ToList())
                    L3.Add(t);
            }
            else if (L2.Count == 0)
            {
                foreach (T t1 in L1)
                {
                    L3.Add(t1);
                }
            }
        }

        /// <summary>
        /// Create a list of only unique members from a collection of members that contain similar members.
        /// </summary>[0 right, 1 left, 2 top, 3 bot, 4 front, 5 back] 
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="comapredMember"></param>
        /// <returns></returns>
        public bool ListMemberExists<T>(List<T> list, T comapredMember)
        {
            bool b = false;
            if (list.Count == 0) b = false;
            else
            {
                foreach (T member in list)
                {
                    if (member.ToString() == comapredMember.ToString())
                    {
                        b = true;
                    }
                }
            }
            return b;
        }
    }
}
