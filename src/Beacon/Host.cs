﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Nancy;
using Nancy.Hosting.Self;
using Serilog;

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
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.Elasticsearch()
                 .WriteTo.ColoredConsole()
                 .CreateLogger();

            const string configstr = @"
					akka {
                        loggers = [""AkkaSemanticLogger.SemanticLogger, AkkaSemanticLogger""]
                        stdout-loglevel = DEBUG
                        loglevel = DEBUG
                        log-config-on-start = on        
						actor {
							provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""       
                            debug {
                                receive = on
                                autoreceive = on
                                lifecycle = on
                                event-stream = on
                                unhandled = on
                            }       
						}
						
						remote {
                            log-remote-lifecycle-events = DEBUG
                            log-received-messages = on
                            akka.remote.log-sent-messages = on
							helios.tcp {
								transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
								applied-adapters = []
								transport-protocol = tcp
								hostname = localhost
								port = 8080
							}
						}            
						
						cluster {
                            log-info = on
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

        public static void StartServices(string serviceName, int port = 0,Action<ActorSystem> configuration = null)
        {
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Elasticsearch()
              .WriteTo.ColoredConsole()
              .CreateLogger();

            var uri = GetUri(port);
            var hostConfigs = GetConfiguration();
            var host = GetHost(uri, hostConfigs);
            using (host)
            {
                Console.WriteLine("Nancy is running on {0}", uri);
                const string configstr = @"
					akka {
                        loggers = [""AkkaSemanticLogger.SemanticLogger, AkkaSemanticLogger""]
                        stdout-loglevel = DEBUG
                        loglevel = DEBUG
                        log-config-on-start = on        
						actor {
							 provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""              
                             debug {
                                receive = on
                                autoreceive = on
                                lifecycle = on
                                event-stream = on
                                unhandled = on
                            }
						}
						
						remote {
                            log-remote-lifecycle-events = DEBUG
                            log-received-messages = on
                            akka.remote.log-sent-messages = on
							helios.tcp {
								transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
								applied-adapters = []
								transport-protocol = tcp
								hostname = localhost
								port = 0
							}
						}            
						
						cluster {
                            log-info = on
							seed-nodes = [""akka.tcp://MyCluster@localhost:8080""] 
							roles = [serviceregistry]
							auto-down-unreachable-after = 10s
						}
					}
";

                var config = ConfigurationFactory.ParseString(configstr);

                using (var system = ActorSystem.Create("MyCluster", config))
                {
                   

                    if (configuration != null)
                    {
                        configuration(system);
                    }
                    else
                    {
                        _client = system.ActorOf(Props.Create(() => new ServiceRegistryActor(serviceName, uri.ToString())), "serviceregistry");

                        Cluster.Get(system)
                            .Subscribe(_client, ClusterEvent.InitialStateAsEvents,
                                new[]
                            {
                                typeof (ClusterEvent.MemberUp), 
                                typeof (ClusterEvent.MemberRemoved),
                                typeof (ClusterEvent.MemberExited)
                            });
                    }

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
            var hostConfigs = new HostConfiguration
            {
                UrlReservations =
                {
                    CreateAutomatically = true
                }
            };
            return hostConfigs;
        }

        private static Uri GetUri(int port = 0)
        {
            port = port == 0 ? FreeTcpPort() : port;
            var uri = new Uri("http://localhost:" + port);
            return uri;
        }

        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}