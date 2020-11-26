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
            return Actor.Done;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new WorkerActor());
            var routerProps = Router.NewRoundRobinPool(props, 3);
            var routerPid = Actor.Spawn(routerProps);

            for (int i = 0; i < 100; i++)
            {
                routerPid.Tell("Hello " + i);
            }
            Console.ReadLine();
        }
    }
}