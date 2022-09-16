// See https://aka.ms/new-console-template for more information
using SubnetCalculator;

Console.WriteLine("Hello, World!");

HttpContext context = new HttpContext();

context.Request.Params.AddItem("supernetip", "10.44.0.0");
context.Request.Params.AddItem("supernetcidr", "24");
context.Request.Params.AddItem("subnetcidr", "28");
context.Request.Params.AddItem("verb", "subnetstocreate");
context.Request.Params.AddItem("ignoreexistsubnets", "");

SubnetsCalculatorProvider provier = new SubnetsCalculatorProvider();

provier.ProcessRequest(context);

Console.WriteLine("End!");