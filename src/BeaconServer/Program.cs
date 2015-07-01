using System;
using Akka.Actor;
using Akka.Cluster;
using Shared;

namespace Beacon
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.StartServer();
        }
    }

}
