using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    public class VectorEntry<TValue>
    {
        /// <summary>
        /// Index of the Entry
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Value of the index
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public VectorEntry() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public VectorEntry(int index, TValue value)
        {
            Index = index;
            Value = value;
        }
    }

    /// <summary>
    /// Extensions for VectorEntry class.
    /// </summary>
    public static class VectorEntryExtensions
    {
        public static SparseVector<double> ToSparseVector(this List<VectorEntry<double>> entries)
        {
            SparseVector<double> vector = new SparseVector<double>();
            foreach (var e in entries)
            {
                if (!vector.ContainsKey(e.Index))
                {
                    vector.Add(e.Index, e.Value);
                }
            }
            return vector;
        }
    }

}
