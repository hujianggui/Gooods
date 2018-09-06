using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    public class MatrixEntry<TValue>
    {
        /// <summary>
        /// The current row index.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// The current column index.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The value at the current index (row, column).
        /// </summary>
        public TValue Value { get; set; }


        public MatrixEntry() {}

        public MatrixEntry(int row, int column, TValue value)
        {
            Row = row;
            Column = column;
            Value = value;
        }
    }
}
