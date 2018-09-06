using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gooods.DataType;

namespace Gooods
{
    /// <summary>
    /// Tests for classes.
    /// </summary>
    public class Tests
    {
        public void Start()
        {
            DenseMatrixTest.Run();
            MatrixExtensionsTest.Run();
            VectorExtensionsTest.Run();
        }

    }
}
