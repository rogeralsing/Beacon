using System;
using Nancy;
using Shared;

namespace ExampleService2
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.StartServices("ExampleService2");
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/"] = _ =>
            {
                return "hello";
            };
        }
    }
}
