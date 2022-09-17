// See https://aka.ms/new-console-template for more information
using SubnetCalculator;

Console.WriteLine("Executing SubnetsCalculator");


string supernetip = args.GetValue(0).ToString();
string supernetcidr = args.GetValue(1).ToString();
string subnetcidr = args.GetValue(2).ToString();

string username = args.GetValue(3).ToString();
string password = args.GetValue(4).ToString();
string host = args.GetValue(5).ToString();

Console.WriteLine("supernetip", supernetip);
Console.WriteLine("supernetcidr", supernetcidr);
Console.WriteLine("subnetcidr", subnetcidr);

HttpContext context = new HttpContext();

context.Request.Params.AddItem("supernetip", supernetip);
context.Request.Params.AddItem("supernetcidr", supernetcidr);
context.Request.Params.AddItem("subnetcidr", subnetcidr);
context.Request.Params.AddItem("verb", "subnetstocreate");
context.Request.Params.AddItem("ignoreexistsubnets", "");

SubnetsCalculatorProvider provier = new SubnetsCalculatorProvider();

SWExecute execute = new SWExecute(host, username, password);

provier.execute = execute;

provier.ProcessRequest(context);
