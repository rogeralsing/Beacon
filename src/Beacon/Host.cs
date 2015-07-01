using System;
using System.Net;
using System.Net.Sockets;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Nancy;
using Nancy.Hosting.Self;
using System.Threading.Tasks;

namespace Shared
{
    public static class Host
    {
        private static IActorRef _client;

        public static async Task<Service[]> FindServices()
        {
            var res = await _client.Ask<FoundServices>(new FindServices());
            return res.Services;
        }
        public static async Task<Service> FindService(string name)
        {
            var res = await _client.Ask<FoundService>(new FindService(name));
            return res.Service;
        }

        public static void StartServer()
        {
            const string configstr = @"
					akka {
            stdout-loglevel = ERROR
            loglevel = ERROR
            log-config-on-start = on        
						actor {
							provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""              
						}
						
						remote {
							helios.tcp {
								transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
								applied-adapters = []
								transport-protocol = tcp
								hostname = localhost
								port = 8080
							}
						}            
						
						cluster {
							seed-nodes = [""akka.tcp://MyCluster@localhost:8080""] 
							roles = [serviceregistry]
							auto-down-unreachable-after = 10s
						}
					}
";

            var config = ConfigurationFactory.ParseString(configstr);

            using (var system = ActorSystem.Create("MyCluster", config))
            {
                Console.ReadLine();
            }
        }


        public static void StartServices(string serviceName)
        {
            var uri = GetUri();
            var hostConfigs = GetConfiguration();
            var host = GetHost(uri, hostConfigs);
            using (host)
            {
                Console.WriteLine("Nancy is running on {0}", uri);
                const string configstr = @"
					akka {
            stdout-loglevel = ERROR
            loglevel = ERROR
            log-config-on-start = on        
						actor {
							provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""              
						}
						
						remote {
							helios.tcp {
								transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
								applied-adapters = []
								transport-protocol = tcp
								hostname = localhost
								port = 0
							}
						}            
						
						cluster {
							seed-nodes = [""akka.tcp://MyCluster@localhost:8080""] 
							roles = [serviceregistry]
							auto-down-unreachable-after = 10s
						}
					}
";

                var config = ConfigurationFactory.ParseString(configstr);

                using (var system = ActorSystem.Create("MyCluster", config))
                {
                    _client = system.ActorOf(Props.Create(() => new ServiceRegistryActor(serviceName, uri.ToString())), "serviceregistry");
                    Cluster.Get(system).Subscribe(_client, ClusterEvent.InitialStateAsEvents, new[] { typeof(ClusterEvent.MemberUp), typeof(ClusterEvent.MemberRemoved), typeof(ClusterEvent.MemberExited) });


                    Console.ReadLine();
                }
            }
        }
        private static NancyHost GetHost(Uri uri, HostConfiguration hostConfigs)
        {
            while (true)
            {
                try
                {
                    var host = new NancyHost(uri, new DefaultNancyBootstrapper(), hostConfigs);
                    host.Start();
                    return host;
                }
                catch
                {
                    Console.WriteLine("Port allocation failed, retrying.");
                }
            }
        }

        private static HostConfiguration GetConfiguration()
        {
            HostConfiguration hostConfigs = new HostConfiguration
            {
                UrlReservations =
                {
                    CreateAutomatically = true
                }
            };
            return hostConfigs;
        }

        private static Uri GetUri()
        {
            var port = FreeTcpPort();
            var uri = new Uri("http://localhost:" + port);
            return uri;
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
