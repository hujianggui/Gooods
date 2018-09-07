using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    /// <summary>
    /// Sparse vector
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SparseVector<TValue> : Dictionary<int, TValue>
    {
        /// <summary>
        /// Transfrom to list of vector entries.
        /// </summary>
        /// <returns></returns>
        public List<VectorEntry<TValue>> ToEntries()
        {
            List<VectorEntry<TValue>> list = new List<VectorEntry<TValue>>();            
            foreach(int i in Keys)
            {
                list.Add(new VectorEntry<TValue>(i, this[i]));
            }
            return list;
        }

    }
}
