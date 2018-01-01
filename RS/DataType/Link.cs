namespace RS.DataType
{
    public class Link
    {
        public int From { get; set; }
        public int To { get; set; }
        public double Weight { get; set; }  

        public Link(int from, int to)
        {
            From = from;
            To = to;
        }

        public Link(int from, int to, double weight)
        {
            From = from;
            To = to;
            Weight = weight;
        }
    }
}
