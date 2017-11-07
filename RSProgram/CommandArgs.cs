using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RSProgram
{
    /// <summary>
    /// Contains the parsed command line arguments. This consists of two
    /// lists, one of argument pairs, and one of stand-alone arguments.
    /// </summary>
    public class CommandArgs
    {
        /// <summary>
        /// Returns the dictionary of argument/value pairs.
        /// </summary>
        public Dictionary<string, string> ArgPairs
        {
            get { return mArgPairs; }
        }

        Dictionary<string, string> mArgPairs = new Dictionary<string, string>();


        List<string> mParams = new List<string>();
        /// <summary>
        /// Returns the list of stand-alone parameters.
        /// </summary>
        public List<string> Params
        {
            get { return mParams; }
        }
    }
}
