using System;
using System.Threading.Tasks;
using Proto;
using Proto.Router;

namespace LimitConcurrency
{
    public class WorkerActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            Console.WriteLine($"{context.Self.Id} Got Message {context.Message}");
            return Task.CompletedTask;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var system = new ActorSystem();
            var props = Props.FromProducer(() => new WorkerActor());
            var routerProps = system.Root.NewRoundRobinPool(props, 3);
            var routerPid = system.Root.SpawnNamed(routerProps,"MyRouter");

            for (int i = 0; i < 100; i++)
            {
                system.Root.Send(routerPid,"Hello " + i);
            }
            Console.ReadLine();
        }
    }
}