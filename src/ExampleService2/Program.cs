using Nancy;
using Shared;

namespace ExampleService2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Host.StartServices("ExampleService2");
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