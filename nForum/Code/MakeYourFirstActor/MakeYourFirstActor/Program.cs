﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
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
                helloActor.Tell(new Hello("Swenug"));
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
