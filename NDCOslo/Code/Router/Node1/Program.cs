using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;

namespace Node1
{
    public class SomeActor : ReceiveActor
    {
        public SomeActor()
        {
            Receive<string>(message => Console.WriteLine("{0} got {1}", Self.Path.ToStringWithAddress(), message));
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {  
    actor {
        deployment {
            /localactor {
                router = round-robin-pool
                nr-of-instances = 5
            }
        }
    }
}
");
            using (var system = ActorSystem.Create("system1", config))
            {
                //create a local group router (see config)
                var local = system.ActorOf(Props.Create(() => new SomeActor()).WithRouter(FromConfig.Instance),
                    "localactor");

                //these messages should reach the workers via the routed local ref
                local.Tell("Local message 1");
                local.Tell("Local message 2");
                local.Tell("Local message 3");
                local.Tell("Local message 4");
                local.Tell("Local message 5");

                Console.ReadLine();
            }
        }
    }
}