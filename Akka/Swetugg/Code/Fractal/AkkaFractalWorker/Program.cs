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
            using (var system = ActorSystem.Create("worker"))
            {
                Console.ReadLine();
            }
        }
    }
}
