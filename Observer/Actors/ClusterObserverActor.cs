using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

namespace Shared
{
    public class ActorResource
    {
        public ActorResource(string name,string fullName,string group)
        {
            Name = name;
            FullName = fullName;
            Group = group;
            _children = new Dictionary<string,ActorResource>();
        }

        public string FullName { get; set; }
        public string Name { get;private set; }
        public string Group { get; set; }

        private readonly Dictionary<string, ActorResource> _children;

        public ActorResource AddChild(string name,string fullName,string group)
        {
            if (_children.ContainsKey(name))
                return _children[name];

            var child = new ActorResource(name,fullName, group);
            _children.Add(name, child);
            return child;
        }

        public IList<ActorResource> Children
        {
            get
            {
                return _children.Values.ToList();
            }
        }
    }
    public class ClusterObserverActor : ReceiveActor
    {
        private ILoggingAdapter _log = Context.GetLogger();
        private readonly ActorResource _root = new ActorResource("Cluster","Cluster","root");
        public ClusterObserverActor()
        {
            Receive<string>(s =>
            {
                Sender.Tell(_root);
            });
            Receive<ClusterEvent.MemberUp>(msg => { PrintClusterEvent(msg); });
            Receive<ClusterEvent.MemberExited>(msg => { PrintClusterEvent(msg); });
            Receive<ClusterEvent.MemberRemoved>(msg => { PrintClusterEvent(msg); });
            Receive<ActorIdentity>(id =>
            {
                if (id.Subject == null)
                    return;

                ReportActor(id);

                var childrenPath = id.Subject.Path + "/*";
                Context.ActorSelection(childrenPath).Tell(new Identify(Guid.NewGuid()));
            });
        }

        private  void ReportActor(ActorIdentity id)
        {
            var path = id.Subject.Path;

            ActorResource current = _root;
            current = current.AddChild(path.Address.System, "System:" + path.Address.System,path.Address.Host);
            current = current.AddChild("Node:" + path.Address.Port, "Port:" + path.Address.Port, path.Address.Port.ToString());
            foreach (var e in path.Elements)
            {
                current = current.AddChild(e, current.FullName + "/" + e, path.Address.Port.ToString());
            }


            _log.Info("Actor found : {IdentifiedActor}", id.Subject);
        }

        private void PrintClusterEvent(ClusterEvent.MemberStatusChange msg)
        {
            _log.Info("Member {MemberStatus} {MemberAddress} {MemberRoles}", msg.Member.Status, msg.Member.Address,
                string.Join(",", msg.Member.Roles));
            
            var address = msg.Member.Address;
            var userPath = $"akka.tcp://{address.System}@{address.Host}:{address.Port}/user";
            Context.ActorSelection(userPath).Tell(new Identify(Guid.NewGuid()));
            var remotePath = $"akka.tcp://{address.System}@{address.Host}:{address.Port}/remote";
            Context.ActorSelection(remotePath).Tell(new Identify(Guid.NewGuid()));
            var sysPath = $"akka.tcp://{address.System}@{address.Host}:{address.Port}/system";
            Context.ActorSelection(sysPath).Tell(new Identify(Guid.NewGuid()));
        }
    }
}