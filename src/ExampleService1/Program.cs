using System;
using Nancy;
using Shared;

namespace SelfHostLabb1
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.StartServices("MyService" + DateTime.Now);
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule ()
        {
            Get["/"] = _ =>
            {
                return "hello";
            };
        }
    }
}
