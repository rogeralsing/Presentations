using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Shared;

namespace Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {  
    log-config-on-start = on
    stdout-loglevel = DEBUG
    loglevel = ERROR
    actor {
        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
        


        deployment {
            /localactor {
            }
            /remoteactor {
                remote = ""akka.tcp://system2@localhost:8080""
            }
        }
    }
    remote {
        helios.tcp {
            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
		    applied-adapters = []
		    transport-protocol = tcp
		    port = 8090
		    hostname = localhost
        }
    }
}
");
            using (var system = ActorSystem.Create("system1", config))
            {
                //create a local group router (see config)
                var local = system.ActorOf(Props.Create(() => new SomeActor()), "localactor");

                //these messages should reach the workers via the routed local ref
                local.Tell("Local message 1");
                local.Tell("Local message 2");
                local.Tell("Local message 3");
                local.Tell("Local message 4");
                local.Tell("Local message 5");

                //create a remote deployed actor
                var remote = system.ActorOf(Props.Create(() => new SomeActor()), "remoteactor");

                //this should reach the remote deployed ref
                remote.Tell("Remote message 1");
                remote.Tell("Remote message 2");
                remote.Tell("Remote message 3");
                remote.Tell("Remote message 4");
                remote.Tell("Remote message 5");

                Console.ReadLine();
            }
        }
    }
}
