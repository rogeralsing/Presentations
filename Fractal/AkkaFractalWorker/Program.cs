using Akka;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaFractalWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = FluentConfig.Begin()
                .StartRemotingOn("127.0.0.1", 8090)
                .Build();

            using (var system = ActorSystem.Create("worker", config))
            {
                Console.ReadLine();
            }
        }
    }
}
