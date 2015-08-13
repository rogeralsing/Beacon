using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;

namespace Shared
{
    public class ActorResource
    {
        public ActorResource(string name)
        {
            Name = name;
            _children = new Dictionary<string,ActorResource>();
        }
        public string Name { get;private set; }

        private readonly Dictionary<string, ActorResource> _children;

        public ActorResource AddChild(string name)
        {
            if (_children.ContainsKey(name))
                return _children[name];

            var child = new ActorResource(name);
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
        private readonly ActorResource _root = new ActorResource("Cluster");
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
            current = current.AddChild(path.Address.System);
            current = current.AddChild(path.Address.Port.ToString());
            foreach (var e in path.Elements)
            {
                current = current.AddChild(e);
            }
            

            Console.WriteLine("   Actor found : {0}", id.Subject);
        }

        private void PrintClusterEvent(ClusterEvent.MemberStatusChange msg)
        {
            Console.WriteLine("Member {0} {1} {2}", msg.Member.Status, msg.Member.Address,
                string.Join(",", msg.Member.Roles));

            var address = msg.Member.Address;
            var userPath = string.Format("akka.tcp://{0}@{1}:{2}/user", address.System, address.Host, address.Port);
            Context.ActorSelection(userPath).Tell(new Identify(Guid.NewGuid()));
            var remotePath = string.Format("akka.tcp://{0}@{1}:{2}/remote", address.System, address.Host, address.Port);
            Context.ActorSelection(remotePath).Tell(new Identify(Guid.NewGuid()));
            var sysPath = string.Format("akka.tcp://{0}@{1}:{2}/system", address.System, address.Host, address.Port);
            Context.ActorSelection(sysPath).Tell(new Identify(Guid.NewGuid()));
        }
    }
}