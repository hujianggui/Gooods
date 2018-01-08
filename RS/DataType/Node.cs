using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.DataType
{
    /// <summary>
    /// Node is used to presente vertexes of a graph. G = (V, E), where V = List<Node>, E = List<Link>.
    /// </summary>
    public class Node
    {
        public int Id { get; private set; }

        public double Weight { get; set; }

        public string Description { get; set; }

        public Node(int id, double weight = 1.0)
        {
            Id = id;
            Weight = weight;
        }

        public Node(int id, double weight, string description)
        {
            Id = id;
            Weight = weight;
            Description = description;
        }
    }
}
