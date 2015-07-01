using Nancy;
using Shared;

namespace ExampleService1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Host.StartServices("ExampleService1");
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/"] = _ => { return "hello"; };
        }
    }
}