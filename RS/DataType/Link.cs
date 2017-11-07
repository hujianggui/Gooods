namespace RS.DataType
{
    public class Link
    {
        public int From = 0;
        public int To = 0;
        public double Weight = 1;   // default

        public Link(int from, int to)
        {
            this.From = from;
            this.To = to;
        }

        public Link(int from, int to, double weight)
        {
            this.From = from;
            this.To = to;
            this.Weight = weight;
        }
    }
}
