namespace SubnetCalculator
{
    public class Request
    {

        public Request()
        {
            Params = new Params();
        }
        public Params Params { get; internal set; }
    }
}