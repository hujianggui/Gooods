namespace RS.DataType
{
    /// <summary>
    /// Link is used to presente edges of a graph. G = (V, E), where V = List<Node>, E = List<Link>.
    /// </summary>
    public class Link
    {
        public int From { get; set; }
        public int To { get; set; }
        public double Weight { get; set; }  


        public Link(int from, int to, double weight = 1.0)
        {
            From = from;
            To = to;
            Weight = weight;
        }
    }
}
