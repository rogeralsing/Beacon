using System;
using Akka.Actor;
using Akka.Cluster;

namespace BeaconServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("MyCluster"))
            {
                Console.ReadLine();
            }
        }
    }
}
