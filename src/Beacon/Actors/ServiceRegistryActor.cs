﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

namespace Shared
{
    public class ServiceRegistryActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<string, Service> _name2Url = new Dictionary<string, Service>();

        public ServiceRegistryActor(string name, string url)
        {
            Receive<FindServices>(msg =>
            {
                var services = new FoundServices(_name2Url.Values);
                Sender.Tell(services);
            });
            Receive<FindService>(msg =>
            {
                if (_name2Url.ContainsKey(msg.Name))
                {
                    var service = _name2Url[msg.Name];
                    Sender.Tell(new FoundService(service));
                }
                else
                {
                    Sender.Tell(new FoundService(null));
                }
            });
            Receive<RegisterService>(msg =>
            {
                if (!_name2Url.ContainsKey(msg.Name))
                {
                    _name2Url.Add(msg.Name, new Service(msg.Name, null));
                }
                _name2Url[msg.Name].AddUrl(msg.Url);
                PrintServices();
            });
            Receive<ClusterEvent.MemberUp>(msg =>
            {
                if (msg.Member.Roles.Contains("serviceregistry"))
                {
                    _log.Info("New service registry found, registering self");
                    var path = msg.Member.Address.ToString() + "/user/serviceregistry";
                    Context.ActorSelection(path).Tell(new RegisterService(name, url));
                    //Console.WriteLine(path);
                }
                PrintClusterEvent(msg);
            });
            Receive<ClusterEvent.MemberExited>(msg => { PrintClusterEvent(msg); });
            Receive<ClusterEvent.MemberRemoved>(msg => { PrintClusterEvent(msg); });
        }

        private void PrintServices()
        {
            Console.WriteLine("-----------------");
            foreach (var kvp in _name2Url)
            {
                foreach (var url in kvp.Value.URLs)
                {
                    _log.Info("Service {ServiceName} - {ServiceURL}", kvp.Key, url);
                }
            }
            Console.WriteLine("-----------------");
        }

        private void PrintClusterEvent(ClusterEvent.MemberStatusChange msg)
        {

            _log.Info("Member {MemberStatus} {MemberAddress} {MemberRoles}", msg.Member.Status, msg.Member.Address,
                string.Join(",", msg.Member.Roles));
        }
    }
}