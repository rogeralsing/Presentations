using System;
using Akka.Actor;

namespace MakeYourFirstActor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("mySystem"))
            {
                var helloActor = system.ActorOf<HelloActor>();
                helloActor.Tell(new Hello("NDCOslo"));
                Console.ReadLine();
            }
        }
    }

    class Hello
    {
        public readonly string Who;
        public Hello(string who)
        {
            Who = who;
        }
    }

    class HelloActor : ReceiveActor
    {
        public HelloActor()
        {
            Receive<Hello>(hello => Console.WriteLine("Hello {0}", hello.Who));
        }
    }
}
