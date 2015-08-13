using System.Threading;
using Akka.Actor;
using Akka.Cluster;
using Nancy;
using Shared;

namespace Observer
{
    internal static class Actors
    {
        internal static IActorRef ClusterObserver { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Host.StartServices("customers",8090, sys =>
            {
                Actors.ClusterObserver = sys.ActorOf<ClusterObserverActor>();
                Cluster.Get(sys)
                .Subscribe(Actors.ClusterObserver, ClusterEvent.InitialStateAsEvents,
                    new[]
                            {
                                typeof (ClusterEvent.MemberUp), 
                                typeof (ClusterEvent.MemberRemoved),
                                typeof (ClusterEvent.MemberExited)
                            });
            });
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/cluster/", true] = async (parameters, ct) =>
            {
                var result = await Actors.ClusterObserver.Ask<ActorResource>("");
                return Response.AsJson(result);
            };

            Get[@"/"] = parameters => Response.AsFile("Content/index.html", "text/html");

        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}