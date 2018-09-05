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
    public class SparseVector<TValue> : Dictionary<int, TValue> { }
}
