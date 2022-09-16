namespace SubnetCalculator
{
    public class HttpContext
    {
        public HttpContext()
        {
            Request = new Request();
            Response = new Response();
        }
        public Response Response { get; internal set; }
        public Request Request { get; internal set; }
    }
}